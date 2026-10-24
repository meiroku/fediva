using Fediva.Application.Ports.Persistence;
using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Users;

namespace Fediva.Application.Identity.Queries;

/// <summary>
/// ユーザー取得クエリ
/// </summary>
/// <param name="UserId">取得対象のユーザーID</param>
public sealed record GetUserQuery(UserId UserId) : IQuery<GetUserDto>;

/// <summary>
/// ユーザー取得クエリハンドラー
/// </summary>
public sealed class GetUserHandler(IUserRepository userRepository)
    : IQueryHandler<GetUserQuery, GetUserDto>
{
    public async Task<Result<GetUserDto>> HandleAsync(
        GetUserQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var user = await userRepository.FindByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            return Result<GetUserDto>.Failure(IdentityErrors.UserNotFound);
        }

        var dto = new GetUserDto(
            user.Id,
            user.DisplayName,
            user.EmailAddress,
            user.Status,
            user.CreatedAt
        );

        return Result<GetUserDto>.Success(dto);
    }
}
