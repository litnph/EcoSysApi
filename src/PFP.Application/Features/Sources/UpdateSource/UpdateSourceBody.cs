using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.UpdateSource;

/// <summary>JSON body for <c>PUT /finance/sources/:id</c> — excludes the route <c>id</c>.</summary>
public sealed record UpdateSourceBody(
    string Name,
    SourceType Type,
    decimal? CreditLimit,
    int? StatementDay,
    int? PaymentDueDay,
    decimal? MinInstallmentAmt,
    string? Currency,
    string? Icon,
    string? Color,
    int? SortOrder);
