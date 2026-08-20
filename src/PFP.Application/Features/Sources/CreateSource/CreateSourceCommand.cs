using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.CreateSource;

/// <summary>Creates a source in the finance module.</summary>
public sealed record CreateSourceCommand(
    string Name,
    SourceType Type,
    decimal? CreditLimit,
    int? StatementDay,
    int? PaymentDueDay,
    decimal? MinInstallmentAmt,
    string? Currency,
    string? Icon,
    string? Color,
    int? SortOrder,
    long? InitialBalance) : IRequest<CreateSourceResponse>;
