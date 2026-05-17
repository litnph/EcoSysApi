using MediatR;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Savings.WithdrawFromSaving;

public sealed record WithdrawFromSavingCommand(
    Guid SavingId,
    decimal Amount,
    DateOnly TxnDate,
    string? Note,
    Guid? MonthlyPeriodId) : IRequest<WithdrawFromSavingResponse>;
