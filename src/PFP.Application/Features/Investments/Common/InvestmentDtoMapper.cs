using PFP.Domain.Entities.Finance;

namespace PFP.Application.Features.Investments.Common;

internal static class InvestmentDtoMapper
{
    public static InvestmentListItemDto ToListItem(FinInvestment i)
    {
        var pnl = i.CurrentValue + i.TotalReturned - i.TotalInvested;
        return new InvestmentListItemDto(
            i.Id,
            i.SmoduleId,
            i.Name,
            i.Type,
            i.CurrentValue,
            i.TotalInvested,
            i.TotalReturned,
            i.Currency,
            i.Note,
            pnl);
    }

    public static InvestmentTxnDto ToTxnDto(FinInvestmentTxn t) =>
        new(t.Id, t.TxnType, t.Amount, t.Quantity, t.PricePerUnit, t.TxnDate, t.Note, t.LinkedTxnId);

    public static InvestmentDetailDto ToDetail(FinInvestment i, IReadOnlyList<InvestmentTxnDto> txns)
    {
        var pnl = i.CurrentValue + i.TotalReturned - i.TotalInvested;
        return new InvestmentDetailDto(
            i.Id,
            i.SmoduleId,
            i.Name,
            i.Type,
            i.CurrentValue,
            i.TotalInvested,
            i.TotalReturned,
            i.Currency,
            i.Note,
            pnl,
            txns,
            i.CreatedAt,
            i.UpdatedAt);
    }
}
