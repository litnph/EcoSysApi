using PFP.Application.Features.Investments.Common;

namespace PFP.Application.Features.Investments.GetInvestments;

public sealed record GetInvestmentsResponse(IReadOnlyList<InvestmentListItemDto> Items);
