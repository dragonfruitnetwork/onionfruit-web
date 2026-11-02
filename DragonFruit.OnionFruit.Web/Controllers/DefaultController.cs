// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DragonFruit.OnionFruit.Web.Controllers;

[ApiController, Route("~/")]
public class DefaultController : ControllerBase
{
    // The OnionFruit logo: wifi arcs over a padlock (kept as-is from the original)
    private static readonly string[] Logo =
    [
        "",
        "",
        "             **************",
        "        +**********************+",
        "     +****************************+",
        "   **********************************",
        "  ************************************",
        " +************************************+",
        "  ************************************",
        "   =************************+     :*-",
        "     ********************+",
        "      .*****************      .+=",
        "        ***************      **=+**",
        "          ************      =*   -*",
        "           =**********      =*   -*",
        "             *********     **********",
        "               *******     **********",
        "                ******     **********",
        "                  ****     **********",
        "                           *********.",
        "",
        "",
    ];

    [HttpGet]
    public IActionResult Index() => Ok(RenderIndex(Program.Version));

    [HttpGet("sys/version")]
    public ActionResult<ServerVersion> ServerVersionInfo()
    {
        return Ok(new ServerVersion(Program.Version));
    }

    /// <summary>
    /// Renders the logo with the program name and version to the right of it, vertically centred
    /// </summary>
    private static string RenderIndex(string version)
    {
        var info = new[] {"OnionFruit™ Web", $"Version {version}"};
        var textRow = (Logo.Length - info.Length) / 2;
        var maxWidth = Logo.Max(row => row.Length);

        var lines = Logo.Select((row, i) =>
            i >= textRow && i < textRow + info.Length
                ? row.PadRight(maxWidth) + "    " + info[i - textRow]
                : row.TrimEnd());

        return string.Join('\n', lines) + '\n';
    }
}