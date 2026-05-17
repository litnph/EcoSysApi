using MediatR;

namespace PFP.Application.Features.DebtRecords.GetDebtSummary;

public sealed record GetDebtSummaryQuery(Guid SmoduleId) : IRequest<GetDebtSummaryResponse>;
