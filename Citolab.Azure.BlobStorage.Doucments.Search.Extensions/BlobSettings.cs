using System;
using System.Collections.Generic;
using System.Text;

namespace Citolab.Azure.BlobStorage.Search.Extensions
{
    public class BlobSettings
    {
        public string BlobConnectionString { get; }
        public string SearchServiceName { get; }
        public string SearchApiKey { get; }

        public BlobSettings(string blobConnectionString, string searchServiceName, string searchApiKey)
        {
            BlobConnectionString = blobConnectionString;
            SearchServiceName = searchServiceName;
            SearchApiKey = searchApiKey;
        }
    }
}
