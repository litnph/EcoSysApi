using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.Common;

public sealed record InvestmentListItemDto(
    Guid Id,
    Guid SmoduleId,
    string Name,
    InvestmentType Type,
    long CurrentValue,
    long TotalInvested,
    long TotalReturned,
    string Currency,
    string? Note,
    long ProfitLoss);

public sealed record InvestmentTxnDto(
    Guid Id,
    InvestmentTxnType TxnType,
    long Amount,
    decimal? Quantity,
    decimal? PricePerUnit,
    DateOnly TxnDate,
    string? Note,
    Guid? LinkedTxnId);

public sealed record InvestmentDetailDto(
    Guid Id,
    Guid SmoduleId,
    string Name,
    InvestmentType Type,
    long CurrentValue,
    long TotalInvested,
    long TotalReturned,
    string Currency,
    string? Note,
    long ProfitLoss,
    IReadOnlyList<InvestmentTxnDto> Transactions,
    DateTime CreatedAt,
    DateTime UpdatedAt);
