using PFP.Application.Features.Savings.Common;

namespace PFP.Application.Features.Savings.GetSavings;

public sealed record GetSavingsResponse(IReadOnlyList<SavingListItemDto> Items);
