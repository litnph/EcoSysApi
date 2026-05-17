using MediatR;

namespace PFP.Application.Features.Investments.GetInvestments;

public sealed record GetInvestmentsQuery(Guid SmoduleId) : IRequest<GetInvestmentsResponse>;
