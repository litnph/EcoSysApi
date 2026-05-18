using FluentValidation;

namespace PFP.Application.Features.Notifications.GetNotifications;

/// <summary>FluentValidation rules for <see cref="GetNotificationsQuery"/>.</summary>
public sealed class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
{
    /// <summary>Registers field rules.</summary>
    public GetNotificationsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
