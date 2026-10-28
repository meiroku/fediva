namespace Fediva.Domain.Content;

/// <summary>
/// Postの公開範囲を表します。
/// Public以外はAT Protocolへ投影されません(判定はApplicationのProjectionPolicyが行います)。
/// </summary>
public enum Visibility
{
    /// <summary>誰でも閲覧可能</summary>
    Public,

    /// <summary>フォロワーのみ閲覧可能</summary>
    Followers,

    /// <summary>宛先Actorのみ閲覧可能（ダイレクトメッセージ相当）</summary>
    Direct,
}
