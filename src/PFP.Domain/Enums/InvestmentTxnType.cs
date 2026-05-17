namespace PFP.Domain.Enums;

/// <summary>Type of an append-only <c>FIN_INVESTMENT_TXN</c> row.</summary>
public enum InvestmentTxnType
{
    /// <summary>Purchase; increases cost basis.</summary>
    Buy = 1,

    /// <summary>Sale; records proceeds.</summary>
    Sell = 2,

    /// <summary>Dividend / coupon cash return.</summary>
    Dividend = 3,

    /// <summary>Fee charged against the position.</summary>
    Fee = 4,
}
