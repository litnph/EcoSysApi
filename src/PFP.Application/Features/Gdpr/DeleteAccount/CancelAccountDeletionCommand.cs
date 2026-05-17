using MediatR;

namespace PFP.Application.Features.Gdpr.DeleteAccount;

/// <summary>Cancels a pending or grace-period deletion for the current user.</summary>
public sealed record CancelAccountDeletionCommand : IRequest<Unit>;
