using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.Common;

/// <summary>API projection of a <c>FIN_SOURCES</c> row (spec §5.3 — monetary fields as whole units).</summary>
public sealed record FinSourceDto(
    Guid Id,
    string Name,
    SourceType Type,
    long Balance,
    string Currency,
    long? CreditLimit,
    int? StatementDay,
    int? PaymentDueDay,
    long? MinInstallmentAmt,
    string? Icon,
    string? Color,
    int SortOrder,
    int Version);
