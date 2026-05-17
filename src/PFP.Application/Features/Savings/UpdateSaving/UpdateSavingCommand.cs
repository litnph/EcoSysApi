using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Savings.UpdateSaving;

public sealed record UpdateSavingCommand(
    Guid Id,
    Guid SourceId,
    string Name,
    decimal? TargetAmount,
    decimal InterestRate,
    DateOnly StartDate,
    DateOnly? MaturityDate,
    SavingType Type,
    SavingStatus Status,
    string? Note) : IRequest<UpdateSavingResponse>;
