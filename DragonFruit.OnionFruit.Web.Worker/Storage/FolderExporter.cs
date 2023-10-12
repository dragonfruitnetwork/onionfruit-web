using System;
using System.IO;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public class FolderExporter : IDataExporter
{
    public string FolderPath { get; set; }
    
    public Task PerformUpload(IServiceProvider services, IUploadFileSource source)
    {
        return source.CopyToFolder(FolderPath);
    }

    public override string ToString() => $"Local Folder Export (target: {Path.GetFullPath(FolderPath)})";
}