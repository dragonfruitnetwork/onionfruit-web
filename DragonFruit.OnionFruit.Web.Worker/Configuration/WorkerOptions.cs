// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;

namespace DragonFruit.OnionFruit.Web.Worker.Configuration;

public class WorkerOptions
{
    public const string SectionName = "Worker";

    /// <summary>
    /// Map of generator type-name to whether it should run. Generators not listed are enabled by default.
    /// </summary>
    public Dictionary<string, bool> EnabledGenerators { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
