using Microsoft.AspNetCore.Authorization;

namespace PFP.API.Authorization;

/// <summary>Requirement matched when the caller's user id appears in configured platform-admin ids.</summary>
public sealed class PlatformAdminRequirement : IAuthorizationRequirement { }
