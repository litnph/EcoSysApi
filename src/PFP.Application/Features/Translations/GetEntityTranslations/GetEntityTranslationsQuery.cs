using MediatR;

namespace PFP.Application.Features.Translations.GetEntityTranslations;

/// <summary>Returns every stored translation row for one entity instance (admin tooling).</summary>
public sealed record GetEntityTranslationsQuery(string EntityType, Guid EntityId)
    : IRequest<IReadOnlyList<EntityTranslationAdminDto>>;

/// <summary>Raw translation row exposed to admins.</summary>
public sealed record EntityTranslationAdminDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Field,
    string LocaleCode,
    string Value,
    DateTime CreatedAt,
    DateTime UpdatedAt);
