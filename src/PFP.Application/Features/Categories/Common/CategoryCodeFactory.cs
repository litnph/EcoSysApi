namespace PFP.Application.Features.Categories.Common;

/// <summary>Generates a unique <see cref="Domain.Entities.FinCategory.Code"/> under a space-module.</summary>
internal static class CategoryCodeFactory
{
    /// <summary>Returns a new code guaranteed unique by random suffix (under 64 chars).</summary>
    public static string NewUniqueCode() => $"c_{Guid.NewGuid():N}";
}
