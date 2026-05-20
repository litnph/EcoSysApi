using PFP.Application.Common;
using PFP.Domain.Entities.Finance;

namespace PFP.Application.Features.Investments.Common;

internal static class InvestmentDtoMapper
{
    public static InvestmentListItemDto ToListItem(FinInvestment i)
    {
        var pnl = i.CurrentValue + i.TotalReturned - i.TotalInvested;
        return new InvestmentListItemDto(
            i.Id,
            i.Name,
            i.Type,
            CurrencyUnits.ToWhole(i.CurrentValue),
            CurrencyUnits.ToWhole(i.TotalInvested),
            CurrencyUnits.ToWhole(i.TotalReturned),
            i.Currency,
            i.Note,
            CurrencyUnits.ToWhole(pnl));
    }

    public static InvestmentTxnDto ToTxnDto(FinInvestmentTxn t) =>
        new(
            t.Id,
            t.TxnType,
            CurrencyUnits.ToWhole(t.Amount),
            t.Quantity,
            t.PricePerUnit,
            t.TxnDate,
            t.Note,
            t.LinkedTxnId);

    public static InvestmentDetailDto ToDetail(FinInvestment i, IReadOnlyList<InvestmentTxnDto> txns)
    {
        var pnl = i.CurrentValue + i.TotalReturned - i.TotalInvested;
        return new InvestmentDetailDto(
            i.Id,
            i.Name,
            i.Type,
            CurrencyUnits.ToWhole(i.CurrentValue),
            CurrencyUnits.ToWhole(i.TotalInvested),
            CurrencyUnits.ToWhole(i.TotalReturned),
            i.Currency,
            i.Note,
            CurrencyUnits.ToWhole(pnl),
            txns,
            i.CreatedAt,
            i.UpdatedAt);
    }
}
