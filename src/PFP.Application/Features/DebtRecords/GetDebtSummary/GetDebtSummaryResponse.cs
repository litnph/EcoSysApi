namespace PFP.Application.Features.DebtRecords.GetDebtSummary;

public sealed record GetDebtSummaryResponse(
    long TotalBorrowedRemaining,
    long TotalLentRemaining,
    int OverdueBorrowedCount,
    int OverdueLentCount);
