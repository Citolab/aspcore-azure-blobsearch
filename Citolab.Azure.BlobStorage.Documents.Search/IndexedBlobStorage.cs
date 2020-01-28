using System;
using System.Collections.Generic;
using System.Text;

namespace Citolab.Azure.BlobStorage.Search
{
    public class IndexedBlobStorage : IIndexedBlobStorage
    {
        private readonly string _blobConnectionString;
        private readonly string _searchServiceName;
        private readonly string _searchApiKey;

        public IndexedBlobStorage(string blobConnectionString, string searchServiceName, string searchApiKey)
        {
            _blobConnectionString = blobConnectionString;
            _searchServiceName = searchServiceName;
            _searchApiKey = searchApiKey;
        }

        public IndexedWordContainer GetOrCreateContainer(string containername) =>
           new IndexedWordContainer(_blobConnectionString, containername, _searchServiceName,  _searchApiKey);

    }
}
