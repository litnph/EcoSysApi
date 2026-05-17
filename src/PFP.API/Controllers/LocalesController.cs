using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Locales.GetLocales;
using PFP.Application.Features.Locales.GetUiStrings;

namespace PFP.API.Controllers;

/// <summary>Public locale catalog and UI-string bundles for front-end bootstrap.</summary>
[ApiController]
[AllowAnonymous]
public sealed class LocalesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public LocalesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns every configured locale.</summary>
    [HttpGet("api/v1/locales")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LocaleListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LocaleListItemDto>>>> List(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetLocalesQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<IReadOnlyList<LocaleListItemDto>> { Data = rows });
    }

    /// <summary>Returns every key/value pair stored for the requested locale.</summary>
    [HttpGet("api/v1/ui-strings/{locale}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyDictionary<string, string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyDictionary<string, string>>>> GetUiStrings(
        string locale,
        CancellationToken cancellationToken)
    {
        var map = await _mediator.Send(new GetUiStringsForLocaleQuery(locale), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<IReadOnlyDictionary<string, string>> { Data = map });
    }
}