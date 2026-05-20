using MediatR;

namespace PFP.Application.Features.MonthlyPeriods.CloseMonth;

/// <summary>Closes a calendar month for a finance module after billing-cycle validation.</summary>
public sealed record CloseMonthCommand(int Year, int Month) : IRequest<CloseMonthResponse>;
