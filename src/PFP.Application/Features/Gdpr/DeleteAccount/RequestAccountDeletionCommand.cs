using MediatR;

namespace PFP.Application.Features.Gdpr.DeleteAccount;

/// <summary>Starts the GDPR account-deletion flow (email confirmation required).</summary>
public sealed record RequestAccountDeletionCommand(string? Reason) : IRequest<RequestAccountDeletionResponse>;

/// <summary>New <see cref="Domain.Entities.UserDeletionRequest"/> id.</summary>
public sealed record RequestAccountDeletionResponse(Guid RequestId);
