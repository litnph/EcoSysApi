namespace PFP.Domain.Enums;

/// <summary>Asset class of a <c>FIN_INVESTMENTS</c> row.</summary>
public enum InvestmentType
{
    /// <summary>Listed equity.</summary>
    Stock = 1,

    /// <summary>Mutual fund / ETF.</summary>
    Fund = 2,

    /// <summary>Real estate, REIT, land.</summary>
    RealEstate = 3,

    /// <summary>Cryptocurrency.</summary>
    Crypto = 4,

    /// <summary>Other instruments.</summary>
    Other = 99,
}
