namespace Fediva.Domain.Identity.Actors;

/// <summary>
/// Actorの出自(ローカルホストで管理されるか、リモートサーバーから配信されたか)を表します。
/// </summary>
public enum ActorOrigin
{
    /// <summary>ローカルサーバー上でホスト・管理されるActor</summary>
    Local,

    /// <summary>フェデレーション経由で取得されたリモートサーバー上のActor</summary>
    Remote,
}
