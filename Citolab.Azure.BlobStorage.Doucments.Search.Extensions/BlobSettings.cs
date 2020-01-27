using System;
using System.Collections.Generic;
using System.Text;

namespace Citolab.Azure.BlobStorage.Search.Extensions
{
    public class BlobSettings
    {
        public string BlobConnectionString { get; }
        public Uri SearchUrl { get; }
        public string SearchApiKey { get; }

        public BlobSettings(string blobConnectionString, Uri searchUrl, string searchApiKey)
        {
            BlobConnectionString = blobConnectionString;
            SearchUrl = searchUrl;
            SearchApiKey = searchApiKey;
        }
    }
}
