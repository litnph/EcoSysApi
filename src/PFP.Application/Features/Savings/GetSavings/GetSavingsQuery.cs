using MediatR;

namespace PFP.Application.Features.Savings.GetSavings;

public sealed record GetSavingsQuery() : IRequest<GetSavingsResponse>;
