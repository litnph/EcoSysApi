using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.GetSourceById;

/// <summary>Loads a single finance source with its current balance projection.</summary>
public sealed record GetSourceByIdQuery(Guid Id) : IRequest<GetSourceByIdResponse>;
