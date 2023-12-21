// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using DragonFruit.Data;
using DragonFruit.Data.Requests;
using JetBrains.Annotations;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public partial class GenericBlobUploadRequest(string uploadUrl, string fileName, Stream file, [CanBeNull] byte[] checksum) : ApiRequest
{
    private readonly byte[] _checksum = checksum;

    public override string RequestPath => $"{uploadUrl.TrimEnd('/')}/{fileName}";
    public override HttpMethod RequestMethod => HttpMethod.Put;

    [RequestParameter(ParameterType.Header, "If-None-Match")]
    protected string ExcludeIf => "*";

    [RequestBody]
    protected HttpContent BuildRequestContent()
    {
        file.Seek(0, SeekOrigin.Begin);

        var content = new StreamContent(file);
        content.Headers.ContentMD5 = _checksum;
        content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

        return content;
    }
}