using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.GetBillingCycles;

/// <summary>Lists billing cycles for a finance module with optional filters.</summary>
public sealed record GetBillingCyclesQuery(Guid SmoduleId, Guid? SourceId, BillingCycleStatus? Status)
    : IRequest<GetBillingCyclesResponse>;
