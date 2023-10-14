// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using DragonFruit.Data;
using DragonFruit.Data.Requests;
using JetBrains.Annotations;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public class GenericBlobUploadRequest : ApiRequest, IRequestExecutingCallback
{
    private readonly Stream _file;
    private readonly byte[] _checksum;
    private readonly string _fileName, _submissionEndpoint;

    public GenericBlobUploadRequest(string uploadUrl, string fileName, Stream file, [CanBeNull] byte[] checksum)
    {
        _file = file;
        _checksum = checksum;
        _fileName = fileName;
        _submissionEndpoint = uploadUrl;

        Headers.Add(new KeyValuePair<string, string>("If-None-Match", "*"));
    }

    public override string Path => $"{_submissionEndpoint.TrimEnd('/')}/{_fileName}";

    protected override Methods Method => Methods.Put;
    protected override BodyType BodyType => BodyType.Custom;

    protected override HttpContent BodyContent
    {
        get
        {
            var content = new StreamContent(_file);
            content.Headers.ContentMD5 = _checksum;
            content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

            return content;
        }
    }

    void IRequestExecutingCallback.OnRequestExecuting(ApiClient client)
    {
        _file.Seek(0, SeekOrigin.Begin);
    }
}