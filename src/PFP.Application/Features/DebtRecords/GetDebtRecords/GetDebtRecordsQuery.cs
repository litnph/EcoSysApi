using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.GetDebtRecords;

public sealed record GetDebtRecordsQuery(
    Guid SmoduleId,
    DebtDirection? Direction,
    DebtStatus? Status) : IRequest<GetDebtRecordsResponse>;
