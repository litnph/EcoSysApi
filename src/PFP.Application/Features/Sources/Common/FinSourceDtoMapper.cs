using PFP.Domain.Entities;

namespace PFP.Application.Features.Sources.Common;

/// <summary>Maps <see cref="FinSource"/> aggregates to <see cref="FinSourceDto"/>.</summary>
public static class FinSourceDtoMapper
{
    /// <summary>Builds a DTO from the entity; <see cref="FinSourceDto.Balance"/> is rounded to a whole currency unit.</summary>
    public static FinSourceDto ToDto(FinSource entity) =>
        new(
            entity.Id,
            entity.SmoduleId,
            entity.Name,
            entity.Type,
            ToWholeCurrencyUnits(entity.Balance),
            entity.Currency,
            entity.CreditLimit is { } cl ? ToWholeCurrencyUnits(cl) : null,
            entity.StatementDay,
            entity.PaymentDueDay,
            entity.MinInstallmentAmt is { } mi ? ToWholeCurrencyUnits(mi) : null,
            entity.Icon,
            entity.Color,
            entity.SortOrder,
            entity.Version);

    private static long ToWholeCurrencyUnits(decimal balance) =>
        (long)Math.Round(balance, 0, MidpointRounding.AwayFromZero);
}
