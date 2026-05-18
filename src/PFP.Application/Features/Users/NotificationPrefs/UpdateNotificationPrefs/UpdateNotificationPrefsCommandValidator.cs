using FluentValidation;

namespace PFP.Application.Features.Users.NotificationPrefs.UpdateNotificationPrefs;

/// <summary>FluentValidation rules for <see cref="UpdateNotificationPrefsCommand"/>.</summary>
public sealed class UpdateNotificationPrefsCommandValidator : AbstractValidator<UpdateNotificationPrefsCommand>
{
    /// <summary>Registers field rules.</summary>
    public UpdateNotificationPrefsCommandValidator()
    {
        RuleFor(x => x.Preferences).NotNull();
        RuleForEach(x => x.Preferences).ChildRules(p =>
        {
            p.RuleFor(x => x.ModuleCode).IsInEnum();
            p.RuleFor(x => x.Channel).IsInEnum();
            p.RuleFor(x => x.EventType).NotEmpty().MaximumLength(64);
        });
    }
}
