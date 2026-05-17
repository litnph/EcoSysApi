using MediatR;

namespace PFP.Application.Features.Gdpr.DeleteAccount;

/// <summary>Confirms deletion from signed email link.</summary>
public sealed record ConfirmAccountDeletionCommand(string Token) : IRequest<Unit>;
