using System.Globalization;

namespace PFP.Domain.ValueObjects;

/// <summary>
/// Immutable value object pairing a numeric amount with an ISO-4217 currency code.
/// <para>
/// Equality is structural — two <see cref="Money"/> instances with the same <see cref="Amount"/>
/// and case-insensitive <see cref="Currency"/> compare equal regardless of allocation.
/// The Domain layer uses <see cref="Money"/> for invariants that need to compare or aggregate
/// amounts across rows (debt remaining, monthly totals, …) without leaking primitives.
/// </para>
/// <para>
/// Persistence still stores the components as separate columns (<c>amount NUMERIC(18,2)</c>,
/// <c>currency VARCHAR(3)</c>) per spec §3.6 — this type is in-memory only.
/// </para>
/// </summary>
public readonly record struct Money
{
    /// <summary>Default platform currency (Vietnamese đồng).</summary>
    public const string DefaultCurrencyCode = "VND";

    /// <summary>Numeric amount, stored to two decimal places.</summary>
    public decimal Amount { get; }

    /// <summary>ISO-4217 currency code, always upper-case.</summary>
    public string Currency { get; }

    /// <summary>Creates a money instance, normalising the currency code to upper-case.</summary>
    /// <exception cref="ArgumentException">Currency is empty or not 3 characters.</exception>
    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency code is required.", nameof(currency));

        var normalised = currency.Trim().ToUpperInvariant();
        if (normalised.Length != 3)
            throw new ArgumentException("Currency code must be exactly 3 letters.", nameof(currency));

        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
        Currency = normalised;
    }

    /// <summary>Creates a zero-amount money instance in <see cref="DefaultCurrencyCode"/>.</summary>
    public static Money Zero(string currency = DefaultCurrencyCode) => new(0m, currency);

    /// <summary>Adds two amounts. Both operands must share the same <see cref="Currency"/>.</summary>
    /// <exception cref="InvalidOperationException">Currencies differ.</exception>
    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    /// <summary>Subtracts two amounts. Both operands must share the same <see cref="Currency"/>.</summary>
    /// <exception cref="InvalidOperationException">Currencies differ.</exception>
    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    /// <summary>True when <paramref name="left"/> represents strictly more value than <paramref name="right"/>.</summary>
    public static bool operator >(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount > right.Amount;
    }

    /// <summary>True when <paramref name="left"/> represents strictly less value than <paramref name="right"/>.</summary>
    public static bool operator <(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount < right.Amount;
    }

    /// <summary>Greater-or-equal comparison; requires identical currency.</summary>
    public static bool operator >=(Money left, Money right) => left > right || left.Equals(right);

    /// <summary>Less-or-equal comparison; requires identical currency.</summary>
    public static bool operator <=(Money left, Money right) => left < right || left.Equals(right);

    /// <summary>Renders <c>{amount} {currency}</c> using the invariant culture.</summary>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Amount:0.00} {Currency}");

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {left.Currency} vs {right.Currency}.");
    }
}
