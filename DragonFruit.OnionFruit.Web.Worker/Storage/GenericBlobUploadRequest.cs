// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DragonFruit.Data;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public class GenericBlobUploadRequest : ApiRequest
{
    private readonly Stream _file;
    private readonly Task<byte[]> _md5ChecksumTask;
    private readonly string _fileName, _submissionEndpoint;

    public GenericBlobUploadRequest(string uploadUrl, string fileName, Stream file)
    {
        _file = file;
        _fileName = fileName;
        _submissionEndpoint = uploadUrl;

        _md5ChecksumTask = CalculateFileChecksumAsync();

        Headers.Add(new KeyValuePair<string, string>("If-None-Match", "*"));
    }

    public override string Path => $"{_submissionEndpoint.TrimEnd('/')}/{_fileName}";

    protected override Methods Method => Methods.Put;
    protected override BodyType BodyType => BodyType.Custom;

    protected override HttpContent BodyContent
    {
        get
        {
            _file.Seek(0, SeekOrigin.Begin);

            var content = new StreamContent(_file);
            content.Headers.ContentMD5 = _md5ChecksumTask.Result;
            content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

            return content;
        }
    }

    private async Task<byte[]> CalculateFileChecksumAsync()
    {
        byte[] checksum;

        _file.Seek(0, SeekOrigin.Begin);
        using (var md5Provider = MD5.Create())
        {
            checksum = await md5Provider.ComputeHashAsync(_file);
        }

        _file.Seek(0, SeekOrigin.Begin);
        return checksum;
    }
}