using MediatR;

namespace PFP.Application.Features.Translations.CreateTranslation;

/// <summary>Creates one polymorphic translation row.</summary>
public sealed record CreateTranslationCommand(
    string EntityType,
    Guid EntityId,
    string Field,
    string LocaleCode,
    string Value) : IRequest<CreateTranslationResponse>;

/// <summary>Identifier of the new <see cref="Domain.Entities.Translation"/>.</summary>
public sealed record CreateTranslationResponse(Guid Id);
