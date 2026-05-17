using MediatR;

namespace PFP.Application.Features.Sources.DeleteSource;

/// <summary>Soft-deletes a <see cref="Domain.Entities.FinSource"/> when it has no linked transactions.</summary>
public sealed record DeleteSourceCommand(Guid Id) : IRequest<DeleteSourceResponse>;
