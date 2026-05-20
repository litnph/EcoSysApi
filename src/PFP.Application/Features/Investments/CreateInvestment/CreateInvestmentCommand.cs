using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.CreateInvestment;

public sealed record CreateInvestmentCommand(
    string Name,
    InvestmentType Type,
    string? Currency,
    string? Note) : IRequest<CreateInvestmentResponse>;
