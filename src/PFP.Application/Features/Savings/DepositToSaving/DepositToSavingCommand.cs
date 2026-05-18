using MediatR;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Savings.DepositToSaving;

public sealed record DepositToSavingCommand(
    Guid SavingId,
    long Amount,
    DateOnly TxnDate,
    string? Note,
    Guid? MonthlyPeriodId) : IRequest<DepositToSavingResponse>;
