using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Savings.CreateSaving;

public sealed record CreateSavingCommand(
    Guid SmoduleId,
    Guid SourceId,
    string Name,
    long? TargetAmount,
    decimal InterestRate,
    DateOnly StartDate,
    DateOnly? MaturityDate,
    SavingType Type,
    SavingStatus Status,
    string? Note) : IRequest<CreateSavingResponse>;
