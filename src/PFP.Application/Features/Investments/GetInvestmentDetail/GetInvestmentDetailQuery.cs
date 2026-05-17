using MediatR;

namespace PFP.Application.Features.Investments.GetInvestmentDetail;

public sealed record GetInvestmentDetailQuery(Guid Id) : IRequest<GetInvestmentDetailResponse>;
