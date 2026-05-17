using MediatR;

namespace PFP.Application.Features.Investments.DeleteInvestment;

public sealed record DeleteInvestmentCommand(Guid Id) : IRequest<DeleteInvestmentResponse>;
