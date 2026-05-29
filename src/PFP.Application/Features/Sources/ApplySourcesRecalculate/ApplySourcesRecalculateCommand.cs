using MediatR;

namespace PFP.Application.Features.Sources.ApplySourcesRecalculate;

public sealed record ApplySourcesRecalculateCommand(IReadOnlyList<Guid> SourceIds)
    : IRequest<ApplySourcesRecalculateResponse>;

public sealed record ApplySourcesRecalculateResultItem(
    Guid SourceId,
    long PreviousBalance,
    long NewBalance,
    bool Applied);

public sealed record ApplySourcesRecalculateResponse(
    IReadOnlyList<ApplySourcesRecalculateResultItem> Results);
