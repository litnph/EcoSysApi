namespace PFP.API.Models;

/// <summary>Body for POST /api/v1/finance/monthly-periods/close.</summary>
public sealed record CloseMonthRequest(int Year, int Month);
