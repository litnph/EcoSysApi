using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PFP.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Generic EF Core <see cref="ValueConverter{TModel, TProvider}"/> that maps every enum value to its
/// snake_case string representation in the database, and back.
/// <para>
/// Applied globally in <c>ConfigureConventions</c> on <see cref="Persistence.AppDbContext"/> for
/// every enum referenced by the domain — matching the spec's "database column stores the snake_case
/// string" rule (§3.6) and keeping the data human-readable in raw SQL tools.
/// </para>
/// </summary>
/// <typeparam name="TEnum">The .NET enum whose values are being persisted.</typeparam>
public sealed class SnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    /// <summary>Creates the converter; safe to use as the singleton converter for the enum type.</summary>
    public SnakeCaseEnumConverter()
        : base(
            v => SnakeCase.From(v.ToString()),
            s => (TEnum)Enum.Parse(typeof(TEnum), SnakeCase.ToPascal(s)))
    {
    }
}
