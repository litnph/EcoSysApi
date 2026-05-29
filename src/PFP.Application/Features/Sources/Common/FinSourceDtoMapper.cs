using PFP.Application.Common;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Sources.Common;

/// <summary>Maps <see cref="FinSource"/> aggregates to <see cref="FinSourceDto"/>.</summary>
public static class FinSourceDtoMapper
{
    /// <summary>Builds a DTO from the entity; <see cref="FinSourceDto.Balance"/> is rounded to a whole currency unit.</summary>
    public static FinSourceDto ToDto(FinSource entity, long installmentRemainingAmount = 0) =>
        new(
            entity.Id,
            entity.Name,
            entity.Type,
            CurrencyUnits.ToWhole(entity.Balance),
            entity.Currency,
            entity.CreditLimit is { } cl ? CurrencyUnits.ToWhole(cl) : null,
            entity.StatementDay,
            entity.PaymentDueDay,
            entity.MinInstallmentAmt is { } mi ? CurrencyUnits.ToWhole(mi) : null,
            entity.Icon,
            entity.Color,
            entity.SortOrder,
            entity.Version,
            entity.Type == Domain.Enums.SourceType.CreditCard
                ? installmentRemainingAmount
                : 0);
}
