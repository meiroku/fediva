using Fediva.Application.Ports.Persistence;
using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Actors;
using Fediva.Domain.Identity.Users;

namespace Fediva.Application.Identity.Queries;

/// <summary>
/// ユーザーに紐づくアクターID一覧取得クエリ
/// </summary>
/// <param name="UserId">対象のユーザーID</param>
public sealed record ListUserActorsQuery(UserId UserId) : IQuery<IReadOnlyCollection<ActorId>>;

/// <summary>
/// ユーザーに紐づくアクターID一覧取得クエリハンドラー
/// </summary>
public sealed class ListUserActorsHandler(IUserRepository userRepository)
    : IQueryHandler<ListUserActorsQuery, IReadOnlyCollection<ActorId>>
{
    public async Task<Result<IReadOnlyCollection<ActorId>>> HandleAsync(
        ListUserActorsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var user = await userRepository.FindByIdAsync(query.UserId, cancellationToken);
        return user is null
            ? Result<IReadOnlyCollection<ActorId>>.Failure(IdentityErrors.UserNotFound)
            : Result<IReadOnlyCollection<ActorId>>.Success(user.ActorIds);
    }
}
