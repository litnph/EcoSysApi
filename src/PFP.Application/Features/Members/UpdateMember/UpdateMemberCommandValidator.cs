using FluentValidation;

namespace PFP.Application.Features.Members.UpdateMember;

public sealed class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.NewPassword).MinimumLength(8).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.NewPassword));
    }
}
