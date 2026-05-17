using FluentValidation;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.UpdateCategory;

/// <summary>FluentValidation rules for <see cref="UpdateCategoryCommand"/>.</summary>
public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    /// <summary>Registers validation rules.</summary>
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.ParentId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("ParentId must be null or a non-empty GUID.");
        RuleFor(x => x.Icon).MaximumLength(64).When(x => x.Icon is not null);
        RuleFor(x => x.Color).MaximumLength(16).When(x => x.Color is not null);
    }
}
