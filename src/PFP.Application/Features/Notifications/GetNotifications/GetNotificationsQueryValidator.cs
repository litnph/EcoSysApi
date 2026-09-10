using FluentValidation;

namespace PFP.Application.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsQueryValidator() => RuleFor(query => query.Limit).InclusiveBetween(1, 100);
}
