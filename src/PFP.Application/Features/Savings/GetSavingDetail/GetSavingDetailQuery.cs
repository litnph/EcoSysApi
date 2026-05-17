using MediatR;

namespace PFP.Application.Features.Savings.GetSavingDetail;

public sealed record GetSavingDetailQuery(Guid Id) : IRequest<GetSavingDetailResponse>;
