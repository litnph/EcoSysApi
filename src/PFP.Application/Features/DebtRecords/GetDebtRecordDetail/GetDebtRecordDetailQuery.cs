using MediatR;

namespace PFP.Application.Features.DebtRecords.GetDebtRecordDetail;

public sealed record GetDebtRecordDetailQuery(Guid DebtRecordId) : IRequest<GetDebtRecordDetailResponse>;
