using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Auth;
using PFP.Application.Features.Members.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Members.CreateMember;

public sealed class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, CreateMemberResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public CreateMemberCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateMemberResponse> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        var email = AuthEmailNormalizer.Normalize(request.Email);

        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken).ConfigureAwait(false))
            throw new BusinessRuleException("A user with this email already exists.");

        if (request.Role == UserRole.Admin
            && await _db.Users.AnyAsync(u => u.Role == UserRole.Admin && u.IsActive, cancellationToken).ConfigureAwait(false))
        {
            // Allow multiple admins; no restriction unless product requires single admin.
        }

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            IsActive = true,
        };

        _db.Users.Add(user);
        _db.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            LanguageCode = "vi",
            Timezone = "Asia/Ho_Chi_Minh",
            DateFormat = "dd/MM/yyyy",
            Theme = "system",
        });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateMemberResponse(new MemberDetailDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.IsActive,
            user.LastLoginAt,
            user.CreatedAt));
    }
}
