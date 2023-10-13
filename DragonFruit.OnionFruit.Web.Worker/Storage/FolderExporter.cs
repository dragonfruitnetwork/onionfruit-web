using System;
using System.IO;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public class FolderExporter : IDataExporter
{
    public string FolderPath { get; set; }
    public Environment.SpecialFolder? SpecialBasePath { get; set; }
    
    public Task PerformUpload(IServiceProvider services, IUploadFileSource source)
    {
        return source.CopyToFolderAsync(GetFullPath());
    }

    private string GetFullPath() => SpecialBasePath.HasValue
        ? Path.Combine(Environment.GetFolderPath(SpecialBasePath.Value), FolderPath)
        : Path.GetFullPath(FolderPath);

    public override string ToString() => $"Local Folder Export (target: {GetFullPath()})";
}