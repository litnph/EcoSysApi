using MediatR;

namespace PFP.Application.Features.Sources.GetSourceBalanceLedger;

public sealed record GetSourceBalanceLedgerQuery(Guid SourceId) : IRequest<GetSourceBalanceLedgerResponse>;
