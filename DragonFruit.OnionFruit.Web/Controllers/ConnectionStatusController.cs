// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.Web.Models;
using DragonFruit.OnionFruit.Web.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace DragonFruit.OnionFruit.Web.Controllers;

[EnableCors]
[ApiController, Route("connectionStatus")]
public sealed class ConnectionStatusController(IConnectionStatusService connectionStatusService) : ControllerBase
{
    [HttpGet]
    [ResponseCache(NoStore = true)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectionStatus>> Get(CancellationToken ct)
    {
        var cloudflareCountry = Request.Headers.TryGetValue("CF-IPCountry", out var header) ? header.ToString() : null;
        var status = await connectionStatusService.GetStatusAsync(HttpContext.Connection.RemoteIpAddress, cloudflareCountry, ct);

        return Ok(status);
    }
}
