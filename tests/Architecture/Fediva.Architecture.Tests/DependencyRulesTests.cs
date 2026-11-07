// DependencyRulesTests.cs
// 配置先: tests/Architecture/Fediva.Architecture.Tests/DependencyRulesTests.cs
// 出典: Fediva.md 11.1（依存規則）/ 11.2-4（名前空間規約）/ 設計原則 4（Domain に Protocol 固有型を入れない）
//
// 前提:
//   1. このテストプロジェクトが src/ の全プロジェクトを ProjectReference していること。
//      手順 4-1 の 4 本だけでは足りないので、下記で全プロジェクトに広げる:
//        dotnet add tests/Architecture/Fediva.Architecture.Tests reference \
//          src/Core/Fediva.Domain src/Core/Fediva.Application \
//          src/Protocols/Fediva.Protocol.ActivityPub src/Protocols/Fediva.Protocol.ATProto \
//          src/Adapters/Fediva.Adapters.ActivityPub src/Adapters/Fediva.Adapters.ATProto \
//          src/Infrastructure/Fediva.Infrastructure.Persistence src/Infrastructure/Fediva.Infrastructure.Security \
//          src/Interfaces/Fediva.Interfaces.Client src/Interfaces/Fediva.Interfaces.ActivityPub \
//          src/Interfaces/Fediva.Interfaces.ATProto src/Interfaces/Fediva.Interfaces.Admin
//   2. パッケージは TUnit と ArchUnitNET（正式名 TngTech.ArchUnitNET）。
//      版は .csproj に直書きせず Directory.Packages.props で固定する。
//   3. 各対象アセンブリに最低 1 型あること。空アセンブリだと ArchUnitNET の
//      セレクタが空になり検査が無言で素通りするため、手順 3 の
//      ServiceCollectionExtensions（アンカー型）がその役割を果たす。
//
// 検査は 3 層に分ける:
//   A. .csproj の ProjectReference そのもの（11.1 は「ProjectReference と Architecture Tests で強制」が定義）
//      → コードで使っていない参照でも検出できる。手順 4-3 の「わざと違反を入れる」確認はこの層が落とす。
//   B. ArchUnitNET による型レベルの依存（参照があっても使えば即アウトな方向を二重に止める）
//   C. 名前空間・禁止型・Application の公開表面（A/B では表せない規則）

using System.Reflection;
using System.Xml.Linq;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
// 名前衝突の回避:
// - このファイルは namespace Fediva.Architecture.Tests にあるため、単純名 Architecture は
//   型ではなく自名前空間 Fediva.Architecture として解決されてしまう (CS0118)。
//   -> using ArchUnitNET.Domain; は入れず、型は別名 ArchArchitecture でだけ参照する。
// - ArchUnitNET.Loader にも Type という型があり System.Type と衝突する (CS0104)。
//   ArchUnitNET.Domain にも Assembly という型があり System.Reflection.Assembly と衝突する。
//   -> 使う側 (System 側) を別名で固定する。別名は通常の using より優先される。
using ArchArchitecture = ArchUnitNET.Domain.Architecture;
using Assembly = System.Reflection.Assembly;
using Type = System.Type;

namespace Fediva.Architecture.Tests;

public sealed class DependencyRulesTests
{
    // ---- アセンブリ名（= プロジェクト名。11.2-1 で Fediva. 前置が規約） ----
    private const string Domain = "Fediva.Domain";
    private const string Application = "Fediva.Application";
    private const string ProtocolAp = "Fediva.Protocol.ActivityPub";
    private const string ProtocolAt = "Fediva.Protocol.ATProtocol";
    private const string AdaptersAp = "Fediva.Adapters.ActivityPub";
    private const string AdaptersAt = "Fediva.Adapters.ATProtocol";
    private const string InfraPersistence = "Fediva.Infrastructure.Persistence";
    private const string InfraSecurity = "Fediva.Infrastructure.Security";
    private const string InterfacesClient = "Fediva.Interfaces.Client";
    private const string InterfacesAp = "Fediva.Interfaces.ActivityPub";
    private const string InterfacesAt = "Fediva.Interfaces.ATProtocol";
    private const string InterfacesAdmin = "Fediva.Interfaces.Admin";
    private const string ServiceDefaults = "Fediva.ServiceDefaults";
    private const string Relay = "Fediva.Relay";
    private const string AppView = "Fediva.AppView";

    private static readonly string[] AllAssemblies =
    [
        Domain,
        Application,
        ProtocolAp,
        ProtocolAt,
        AdaptersAp,
        AdaptersAt,
        InfraPersistence,
        InfraSecurity,
        InterfacesClient,
        InterfacesAp,
        InterfacesAt,
        InterfacesAdmin,
        ServiceDefaults,
        Relay,
        AppView,
    ];

    // この4つは Phase 0 の中核なので、読み込めなければテスト自体を失敗させる。
    private static readonly string[] CoreAssemblies = [Domain, Application, ProtocolAp, ProtocolAt];

    private static readonly ArchArchitecture Arch = BuildArchitecture();

    private static ArchArchitecture BuildArchitecture()
    {
        var assemblies = new List<Assembly>();
        foreach (var name in AllAssemblies)
        {
            try
            {
                assemblies.Add(Assembly.Load(new AssemblyName(name)));
            }
            catch (FileNotFoundException) when (!CoreAssemblies.Contains(name))
            {
                // まだ作成していないプロジェクト（Standalone 等）は読み飛ばす。
                // 作成してテストプロジェクトから参照した時点で自動的に検査対象になる。
            }
        }

        return new ArchLoader().LoadAssemblies(assemblies.ToArray()).Build();
    }

    // =====================================================================
    // B. 型レベルの依存規則（11.1）
    // =====================================================================

    private static void AssertNotDependsOn(
        string sourceAssembly,
        params string[] forbiddenAssemblies
    )
    {
        foreach (var forbidden in forbiddenAssemblies)
        {
            if (forbidden == sourceAssembly)
            {
                continue;
            }

            // 読み込めていないアセンブリ同士の組は検査できないので飛ばす（A 層が .csproj で担保する）。
            if (!Arch.Assemblies.Any(a => a.Name == forbidden))
            {
                continue;
            }

            IArchRule rule = Types()
                .That()
                .ResideInAssembly(sourceAssembly)
                .Should()
                .NotDependOnAnyTypesThat()
                .ResideInAssembly(forbidden)
                .Because($"Fediva.md 11.1: {sourceAssembly} must not depend on {forbidden}");

            // rule.Check(Arch) は使えない。Check() は xUnit / NUnit / MSTest 用の
            // 拡張パッケージ (TngTech.ArchUnitNET.xUnit 等) が提供するもので、
            // TUnit 用の拡張は存在しない (公式ガイド 4.1 / 4.2)。
            // 拡張なしの場合の公式パターンが HasNoViolations + 自前 Assert で、
            // ここでは失敗理由を保つため例外で落とす。
            if (!rule.HasNoViolations(Arch))
            {
                throw new InvalidOperationException(
                    $"Fediva.md 11.1 violation: {sourceAssembly} must not depend on {forbidden}"
                );
            }
        }
    }

    private static string[] AllExcept(params string[] allowed) =>
        AllAssemblies.Where(a => !allowed.Contains(a)).ToArray();

    [Test]
    public void Domain_ShouldDependOnBclOnly()
    {
        // Domain -> BCL のみ。他の Fediva アセンブリは全部禁止。
        AssertNotDependsOn(Domain, AllExcept(Domain));
    }

    [Test]
    public void Protocols_ShouldDependOnBclOnly()
    {
        // Protocol.* -> BCL のみ。Domain / Application / 他の Protocol も禁止。
        AssertNotDependsOn(ProtocolAp, AllExcept(ProtocolAp));
        AssertNotDependsOn(ProtocolAt, AllExcept(ProtocolAt));
    }

    [Test]
    public void Application_ShouldDependOnlyOnDomain()
    {
        // Application -> Domain のみ。Protocol / Infrastructure の具体型を公開契約に出さない（11.1 注記）。
        AssertNotDependsOn(Application, AllExcept(Application, Domain));
    }

    [Test]
    public void Adapters_ShouldDependOnlyOnApplicationAndOwnProtocol()
    {
        // Adapters.* -> Application + 自身の Protocol.* のみ。他方 Protocol / Infrastructure / Interfaces は禁止。
        // Domain の型は Application 経由の推移的参照で使う。直接参照が必要になったら矩形（A層）ごと ADR で更新する。
        AssertNotDependsOn(AdaptersAp, AllExcept(AdaptersAp, Application, ProtocolAp));
        AssertNotDependsOn(AdaptersAt, AllExcept(AdaptersAt, Application, ProtocolAt));
    }

    [Test]
    public void Infrastructure_ShouldNotDependOnAdaptersInterfacesOrWrongProtocol()
    {
        // Infrastructure.* -> Application（Port 実装）+ 必要な Domain。
        AssertNotDependsOn(InfraPersistence, AllExcept(InfraPersistence, Application, Domain));
        // 例外（11.1）: Infrastructure.Security -> Protocol.ATProto（OAuth 状態の Port）。
        // Infrastructure.ATProtoStorage を作ったときも同じ扱いでここへ追加する。
        AssertNotDependsOn(
            InfraSecurity,
            AllExcept(InfraSecurity, Application, Domain, ProtocolAt)
        );
    }

    [Test]
    public void Interfaces_ShouldNotDependOnAdaptersOrInfrastructure()
    {
        // Interfaces は Adapter の具体実装を参照せず、Host が DI で注入する（11.1 注記）。
        AssertNotDependsOn(InterfacesClient, AllExcept(InterfacesClient, Application));
        AssertNotDependsOn(InterfacesAdmin, AllExcept(InterfacesAdmin, Application));
        // ActivityPub / ATProto は相手 Protocol の「型のみ」可。他方 Protocol は禁止。
        AssertNotDependsOn(InterfacesAp, AllExcept(InterfacesAp, Application, ProtocolAp));
        AssertNotDependsOn(InterfacesAt, AllExcept(InterfacesAt, Application, ProtocolAt));
    }

    [Test]
    public void Standalone_ShouldNotDependOnUnifiedCore()
    {
        // Standalone.* -> Protocol.ATProto + ServiceDefaults のみ。Unified Core を参照しない（11.1）。
        AssertNotDependsOn(Relay, AllExcept(Relay, ProtocolAt, ServiceDefaults));
        AssertNotDependsOn(AppView, AllExcept(AppView, ProtocolAt, ServiceDefaults));
    }

    // =====================================================================
    // A. .csproj の ProjectReference 矩形（11.1 の実体）
    // =====================================================================

    // 許可リスト。「ここに無い Fediva.* への参照 = 違反」。必要になったらテストと一緒に更新し、理由を ADR に残す。
    private static readonly Dictionary<string, string[]> AllowedProjectReferences = new()
    {
        [Domain] = [],
        [Application] = [Domain],
        [ProtocolAp] = [],
        [ProtocolAt] = [],
        [AdaptersAp] = [Application, ProtocolAp],
        [AdaptersAt] = [Application, ProtocolAt],
        [InfraPersistence] = [Application, Domain],
        [InfraSecurity] = [Application, ProtocolAt],
        [InterfacesClient] = [Application],
        [InterfacesAdmin] = [Application],
        [InterfacesAp] = [Application, ProtocolAp],
        [InterfacesAt] = [Application, ProtocolAt],
        [ServiceDefaults] = [],
        [Relay] = [ProtocolAt, ServiceDefaults],
        [AppView] = [ProtocolAt, ServiceDefaults],
        // Hosts（WebHost / Worker / AppHost）は Composition Root として全参照可なので矩形に入れない（11.1）。
    };

    [Test]
    public async Task ProjectReferences_ShouldMatch_Section11_1()
    {
        var srcDir = Path.Combine(FindSolutionRoot(), "src");
        var violations = new List<string>();

        foreach (var (project, allowed) in AllowedProjectReferences)
        {
            var csproj = Directory
                .GetFiles(srcDir, project + ".csproj", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (csproj is null)
            {
                continue; // 未作成のプロジェクトは対象外。作った時点から効く。
            }

            var actual = XDocument
                .Load(csproj)
                .Descendants("ProjectReference")
                .Select(e => (string?)e.Attribute("Include"))
                .Where(include => include is not null)
                .Select(include => Path.GetFileNameWithoutExtension(include!.Replace('\\', '/')))
                .Where(name => name.StartsWith("Fediva.", StringComparison.Ordinal))
                .ToArray();

            violations.AddRange(
                actual
                    .Where(name => name != project && !allowed.Contains(name))
                    .Select(name => $"{project} -> {name}")
            );
        }

        // 違反があれば一覧ごと失敗する。手順 4-3 で Domain -> Application を足すとここに "Fediva.Domain -> Fediva.Application" が出る。
        await Assert.That(violations).IsEmpty();
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Fediva.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException(
                "Fediva.slnx が見つからない。テストはソリューション配下で実行すること。"
            );
    }

    // =====================================================================
    // C. 名前空間規約 / 禁止型 / Application の公開表面
    // =====================================================================

    [Test]
    public async Task Namespaces_ShouldStartWithAssemblyName()
    {
        // 11.2-4: 名前空間は「プロジェクト名 + 領域フォルダ」（例: Fediva.Domain.Content）。
        // したがって全型の名前空間は自アセンブリ名で始まらなければならない（フォルダ一致は要求しない）。
        var violations = Arch
            .Types.Where(t => AllAssemblies.Contains(t.Assembly.Name))
            .Where(t => !string.IsNullOrEmpty(t.Namespace.FullName))
            .Where(t =>
                !t.Namespace.FullName.Equals(t.Assembly.Name, StringComparison.Ordinal)
                && !t.Namespace.FullName.StartsWith(t.Assembly.Name + ".", StringComparison.Ordinal)
            )
            .Select(t => $"{t.FullName} (assembly: {t.Assembly.Name})")
            .ToList();

        await Assert.That(violations).IsEmpty();
    }

    [Test]
    public async Task Domain_ShouldNotContainProtocolSpecificTypes()
    {
        // 設計原則 4: Domain に Protocol 固有型（DID / AT URI / CID / NSID / TID / AP URI の構造）を入れない。
        // Domain が持ってよいのは NetworkKind / ProtocolKind / 不透明な ExternalIdentifier 等だけ。
        string[] forbiddenTypeNames =
        [
            "Did",
            "DidDocument",
            "AtUri",
            "Cid",
            "Nsid",
            "Tid",
            "RecordKey",
            "ActivityPubUri",
        ];

        var violations = Arch
            .Types.Where(t => t.Assembly.Name == Domain)
            .Where(t => forbiddenTypeNames.Contains(t.Name))
            .Select(t => t.FullName)
            .ToList();

        await Assert.That(violations).IsEmpty();
    }

    [Test]
    public async Task Application_PublicSurface_ShouldNotExposeProtocolOrInfrastructureTypes()
    {
        // 11.1 注記: Application の公開契約（公開型の継承・プロパティ・引数・戻り値）に
        // Protocol / Adapter / Infrastructure の型を出さない。内部実装の依存は B 層が別に止める。
        static bool IsForbidden(Assembly assembly)
        {
            var name = assembly.GetName().Name ?? "";
            return name.StartsWith("Fediva.Protocol.", StringComparison.Ordinal)
                || name.StartsWith("Fediva.Adapters.", StringComparison.Ordinal)
                || name.StartsWith("Fediva.Infrastructure.", StringComparison.Ordinal)
                || name.StartsWith("Fediva.Interfaces.", StringComparison.Ordinal);
        }

        var violations = new List<string>();
        foreach (var type in Assembly.Load(new AssemblyName(Application)).GetExportedTypes())
        {
            var signatureTypes = EnumerateSignatureTypes(type).Distinct();
            violations.AddRange(
                signatureTypes
                    .Where(t => IsForbidden(t.Assembly))
                    .Select(t => $"{type.FullName} exposes {t.FullName}")
            );
        }

        await Assert.That(violations).IsEmpty();
    }

    private static IEnumerable<Type> EnumerateSignatureTypes(Type type)
    {
        var candidates = new List<Type?>();

        candidates.AddRange(type.GetInterfaces());
        candidates.AddRange(type.GetProperties().Select(p => p.PropertyType));
        candidates.AddRange(type.GetFields().Select(f => f.FieldType));
        candidates.AddRange(type.GetMethods().Select(m => m.ReturnType));
        candidates.AddRange(
            type.GetMethods().SelectMany(m => m.GetParameters()).Select(p => p.ParameterType)
        );
        candidates.AddRange(
            type.GetConstructors().SelectMany(c => c.GetParameters()).Select(p => p.ParameterType)
        );

        foreach (var candidate in candidates)
        {
            foreach (var expanded in Expand(candidate))
            {
                yield return expanded;
            }
        }
    }

    private static IEnumerable<Type> Expand(Type? type)
    {
        if (type is null)
        {
            yield break;
        }

        yield return type;

        if (type.IsArray && type.GetElementType() is { } element)
        {
            foreach (var expanded in Expand(element))
            {
                yield return expanded;
            }
        }

        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
            {
                foreach (var expanded in Expand(argument))
                {
                    yield return expanded;
                }
            }
        }
    }
}
