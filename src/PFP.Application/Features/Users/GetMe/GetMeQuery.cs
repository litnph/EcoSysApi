using MediatR;
using PFP.Application.Features.Users.Common;

namespace PFP.Application.Features.Users.GetMe;

/// <summary>Returns the authenticated user's summary plus personalisation defaults.</summary>
public sealed record GetMeQuery() : IRequest<GetMeResponse>;

/// <summary>Response wrapper.</summary>
public sealed record GetMeResponse(UserMeDto User);
