// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using DragonFruit.Data;
using DragonFruit.Data.Extensions;
using DragonFruit.Data.Requests;

namespace DragonFruit.OnionFruit.Services.LocationDb.Sync
{
    public class LocationDbDownloadRequest : ApiRequest, IRequestExecutingCallback
    {
        public override string Path => "https://location.ipfire.org/databases/1/location.db.xz";

        /// <summary>
        /// The <see cref="DateTime"/> the previous database was downloaded, if applicable
        /// </summary>
        public DateTime? LastDownload { get; set; }

        void IRequestExecutingCallback.OnRequestExecuting(ApiClient client)
        {
            if (LastDownload.HasValue)
            {
                this.WithHeader("If-Modified-Since", LastDownload.Value.ToString("R"));
            }
        }
    }
}
