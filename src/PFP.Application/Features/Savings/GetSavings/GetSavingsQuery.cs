using MediatR;

namespace PFP.Application.Features.Savings.GetSavings;

public sealed record GetSavingsQuery(Guid SmoduleId) : IRequest<GetSavingsResponse>;
