using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Auth.Common;

/// <summary>
/// Provisions a personal organisation, root space, finance module, and memberships for a new user
/// (register / Google first sign-in). Mirrors non-personal org creation defaults.
/// </summary>
internal static class PersonalOrganizationBootstrap
{
    internal sealed record Result(
        Organization Organization,
        Space RootSpace,
        SpaceModule FinanceModule);

    internal static async Task<Result> ProvisionAsync(
        IApplicationDbContext db,
        Guid userId,
        string fullName,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var orgName = $"{fullName.Trim()}'s Personal";
        var slug = await OrganizationSlugFactory
            .ReserveUniqueSlugAsync(db, fullName, userId, cancellationToken)
            .ConfigureAwait(false);

        var organization = new Organization
        {
            IsPersonal = true,
            Slug = slug,
            Name = orgName,
            OwnerId = userId,
            DefaultCurrency = "VND",
        };
        db.Organizations.Add(organization);

        db.OrgMembers.Add(
            new OrgMember
            {
                OrgId = organization.Id,
                UserId = userId,
                Role = OrgRole.Owner,
                IsActive = true,
                JoinedAt = nowUtc,
            });

        var rootSpace = new Space
        {
            OrgId = organization.Id,
            ParentId = null,
            Name = "Personal",
            Type = SpaceType.Personal,
            Path = $"/{organization.Id}",
            Depth = 0,
            SortOrder = 0,
        };
        db.Spaces.Add(rootSpace);

        var financeModule = new SpaceModule
        {
            SpaceId = rootSpace.Id,
            ModuleCode = ModuleCode.Finance,
            IsEnabled = true,
            EnabledAt = nowUtc,
        };
        db.SpaceModules.Add(financeModule);

        db.SpaceMembers.Add(
            new SpaceMember
            {
                SpaceId = rootSpace.Id,
                UserId = userId,
                Role = SpaceRole.Manager,
                Inherited = false,
                JoinedAt = nowUtc,
            });

        return new Result(organization, rootSpace, financeModule);
    }
}
