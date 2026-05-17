using MediatR;

namespace PFP.Application.Features.Transactions.Splits.GetPendingSplits;

/// <summary>Lists pending split reimbursements for a finance module, grouped by parent transaction.</summary>
public sealed record GetPendingSplitsQuery(Guid SmoduleId) : IRequest<GetPendingSplitsResponse>;
