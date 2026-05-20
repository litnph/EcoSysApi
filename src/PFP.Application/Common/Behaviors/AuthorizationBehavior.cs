using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Common.Behaviors;

/// <summary>MediatR pipeline stage enforcing <see cref="IAuthorizeRequest"/>.</summary>
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationDbContext _db;

    public AuthorizationBehavior(ICurrentUserService currentUser, IApplicationDbContext db)
    {
        _currentUser = currentUser;
        _db = db;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IAuthorizeRequest auth)
        {
            if (auth.RequireAuthenticated && !_currentUser.IsAuthenticated)
                throw new UnauthorizedAppException("Authentication is required.");

            if (auth.RequireAdmin)
            {
                if (_currentUser.UserId is not { } userId)
                    throw new UnauthorizedAppException("Authentication is required.");

                var isAdmin = await _db.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == userId && u.Role == UserRole.Admin && u.IsActive, cancellationToken)
                    .ConfigureAwait(false);

                if (!isAdmin)
                    throw new ForbiddenException("Admin role is required.");
            }
        }

        return await next().ConfigureAwait(false);
    }
}
