using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.UpdateSource;

/// <summary>Updates mutable configuration on an existing <see cref="Domain.Entities.FinSource"/>.</summary>
public sealed record UpdateSourceCommand(
    Guid Id,
    string Name,
    SourceType Type,
    decimal? CreditLimit,
    int? StatementDay,
    int? PaymentDueDay,
    decimal? MinInstallmentAmt,
    string? Currency,
    string? Icon,
    string? Color,
    int? SortOrder) : IRequest<UpdateSourceResponse>;
