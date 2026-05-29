using MediatR;

namespace PFP.Application.Features.Sources.CreateBalanceAdjustment;

public sealed record CreateBalanceAdjustmentCommand(
    Guid SourceId,
    long Amount,
    DateOnly TxnDate,
    string Note) : IRequest<CreateBalanceAdjustmentResponse>;
