using FluentValidation;

namespace PFP.Application.Features.Sources.GetSources;

/// <summary>FluentValidation rules for <see cref="GetSourcesQuery"/>.</summary>
public sealed class GetSourcesQueryValidator : AbstractValidator<GetSourcesQuery>
{
    /// <summary>Registers query rules.</summary>
    public GetSourcesQueryValidator()
    {
    }
}
