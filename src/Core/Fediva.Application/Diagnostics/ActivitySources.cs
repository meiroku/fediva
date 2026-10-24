using System.Diagnostics;

namespace Fediva.Application.Diagnostics;

public static class ActivitySources
{
    /// <summary>
    /// アプリケーション層のトレーシングソース名
    /// </summary>
    public const string ApplicationSourceName = "Fediva.Application";

    /// <summary>
    /// アプリケーション層のデフォルト <see cref="ActivitySource"/>
    /// </summary>
    public static readonly ActivitySource Application = new(ApplicationSourceName);
}
