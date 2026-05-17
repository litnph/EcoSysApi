using PFP.Domain.Enums;
using MediatR;

namespace PFP.Application.Features.Locales.GetLocales;

/// <summary>Lists active platform locales for client bootstrapping.</summary>
public sealed record GetLocalesQuery : IRequest<IReadOnlyList<LocaleListItemDto>>;

/// <summary>One row from <c>LOCALES</c>.</summary>
public sealed record LocaleListItemDto(
    string Code,
    string Name,
    string EnglishName,
    TextDirection Direction,
    bool IsDefault,
    bool IsActive);
