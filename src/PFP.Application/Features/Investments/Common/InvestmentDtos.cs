using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.Common;

public sealed record InvestmentListItemDto(
    Guid Id,
    Guid SmoduleId,
    string Name,
    InvestmentType Type,
    decimal CurrentValue,
    decimal TotalInvested,
    decimal TotalReturned,
    string Currency,
    string? Note,
    decimal ProfitLoss);

public sealed record InvestmentTxnDto(
    Guid Id,
    InvestmentTxnType TxnType,
    decimal Amount,
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
    decimal CurrentValue,
    decimal TotalInvested,
    decimal TotalReturned,
    string Currency,
    string? Note,
    decimal ProfitLoss,
    IReadOnlyList<InvestmentTxnDto> Transactions,
    DateTime CreatedAt,
    DateTime UpdatedAt);
