using FluentValidation;
using System.Text.RegularExpressions;

namespace PFP.Application.Features.Tags.CreateTag;

public sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    private static readonly Regex HexColorRegex = new(@"^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

    public CreateTagCommandValidator()
    {
RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Color)
            .NotEmpty()
            .Length(7)
            .Matches(HexColorRegex);
    }
}
