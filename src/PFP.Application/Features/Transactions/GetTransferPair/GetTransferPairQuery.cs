using MediatR;

namespace PFP.Application.Features.Transactions.GetTransferPair;

/// <summary>Loads both legs of a transfer pair given either transaction id.</summary>
public sealed record GetTransferPairQuery(Guid TransactionId) : IRequest<GetTransferPairResponse>;
