namespace PFP.Domain.Enums;

/// <summary>Controls how a named report month is positioned around its configured cutoff day.</summary>
public enum MonthlyReportPeriodMode
{
    /// <summary>The named month is the exclusive upper boundary (previous cutoff to named-month cutoff).</summary>
    LowerBoundary = 0,

    /// <summary>The named month is the inclusive lower boundary (named-month cutoff to next cutoff).</summary>
    UpperBoundary = 1,
}
