using PFP.Application.Common.Localization;

namespace PFP.Application.Common.Interfaces;

/// <summary>Abstraction over the authenticated user for the current request.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    Guid? SessionId { get; }

    bool IsAuthenticated { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    string CurrentLocale { get; }
}
