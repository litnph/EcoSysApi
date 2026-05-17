using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.UpdateInvestment;

public sealed record UpdateInvestmentCommand(
    Guid Id,
    string Name,
    InvestmentType Type,
    decimal CurrentValue,
    string Currency,
    string? Note) : IRequest<UpdateInvestmentResponse>;
