using PFP.Domain.Enums;

namespace PFP.Application.Features.Savings.Common;

/// <summary>List row for a savings book.</summary>
public sealed record SavingListItemDto(
    Guid Id,
    Guid SmoduleId,
    Guid SourceId,
    string SourceName,
    string Name,
    long? TargetAmount,
    long CurrentAmount,
    decimal InterestRate,
    DateOnly StartDate,
    DateOnly? MaturityDate,
    SavingType Type,
    SavingStatus Status,
    string? Note);

/// <summary>Detail payload including linked source.</summary>
public sealed record SavingDetailDto(
    Guid Id,
    Guid SmoduleId,
    Guid SourceId,
    string SourceName,
    string Name,
    long? TargetAmount,
    long CurrentAmount,
    decimal InterestRate,
    DateOnly StartDate,
    DateOnly? MaturityDate,
    SavingType Type,
    SavingStatus Status,
    string? Note,
    DateTime CreatedAt,
    DateTime UpdatedAt);
