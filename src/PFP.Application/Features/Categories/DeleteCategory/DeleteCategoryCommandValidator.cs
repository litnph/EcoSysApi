using FluentValidation;

namespace PFP.Application.Features.Categories.DeleteCategory;

/// <summary>FluentValidation rules for <see cref="DeleteCategoryCommand"/>.</summary>
public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    /// <summary>Registers validation rules.</summary>
    public DeleteCategoryCommandValidator() =>
        RuleFor(x => x.Id).NotEmpty();
}
