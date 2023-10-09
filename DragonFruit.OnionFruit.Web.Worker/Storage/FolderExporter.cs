using System;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public class FolderExporter : IDataExporter
{
    private readonly string _folderPath;

    public FolderExporter(string folderPath)
    {
        _folderPath = folderPath;
    }
    
    public Task PerformUpload(IServiceProvider services, IUploadFileSource source)
    {
        return source.CopyToFolder(_folderPath);
    }
}