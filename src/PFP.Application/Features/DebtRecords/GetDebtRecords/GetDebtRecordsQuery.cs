using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.GetDebtRecords;

public sealed record GetDebtRecordsQuery(
    DebtDirection? Direction,
    DebtStatus? Status) : IRequest<GetDebtRecordsResponse>;
