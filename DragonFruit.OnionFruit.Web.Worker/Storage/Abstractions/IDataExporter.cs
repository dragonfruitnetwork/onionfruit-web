using System;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public interface IDataExporter
{
    Task PerformUpload(IServiceProvider services, IUploadFileSource source);
}