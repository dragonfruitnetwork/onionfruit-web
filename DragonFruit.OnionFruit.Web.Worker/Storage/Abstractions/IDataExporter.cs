// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;

public interface IDataExporter
{
    Task PerformUpload(IServiceProvider services, IUploadFileSource source);
}