using MediatR;

namespace PFP.Application.Features.Sources.RecalculateSourceBalance;

public sealed record RecalculateSourceBalanceCommand(Guid SourceId) : IRequest<RecalculateSourceBalanceResponse>;
