using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.RecordInvestmentTxn;

public sealed record RecordInvestmentTxnCommand(
    Guid InvestmentId,
    InvestmentTxnType TxnType,
    long Amount,
    decimal? Quantity,
    decimal? PricePerUnit,
    DateOnly TxnDate,
    string? Note,
    Guid? LinkedTxnId) : IRequest<RecordInvestmentTxnResponse>;
