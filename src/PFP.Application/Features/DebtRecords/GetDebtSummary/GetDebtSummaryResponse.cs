namespace PFP.Application.Features.DebtRecords.GetDebtSummary;

public sealed record GetDebtSummaryResponse(
    decimal TotalBorrowedRemaining,
    decimal TotalLentRemaining,
    int OverdueBorrowedCount,
    int OverdueLentCount);
