using MediatR;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Savings.DepositToSaving;

public sealed record DepositToSavingCommand(
    Guid SavingId,
    decimal Amount,
    DateOnly TxnDate,
    string? Note,
    Guid? MonthlyPeriodId) : IRequest<DepositToSavingResponse>;
