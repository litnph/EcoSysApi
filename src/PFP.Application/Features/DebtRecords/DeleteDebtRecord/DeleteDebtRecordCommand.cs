using MediatR;

namespace PFP.Application.Features.DebtRecords.DeleteDebtRecord;

public sealed record DeleteDebtRecordCommand(Guid DebtRecordId) : IRequest<DeleteDebtRecordResponse>;
