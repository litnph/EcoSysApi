using FluentValidation;
using System.Text.RegularExpressions;

namespace PFP.Application.Features.Tags.UpdateTag;

public sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    private static readonly Regex HexColorRegex = new(@"^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

    public UpdateTagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Color)
            .NotEmpty()
            .Length(7)
            .Matches(HexColorRegex);
    }
}
