﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Citolab.Azure.BlobStorage.Search
{
    public class IndexedBlobStorage : IIndexedBlobStorage
    {
        private readonly string _blobConnectionString;
        private readonly Uri _searchUrl;
        private readonly string _searchApiKey;


        public IndexedBlobStorage(string blobConnectionString, Uri searchUrl, string searchApiKey)
        {
            _blobConnectionString = blobConnectionString;
            _searchUrl = searchUrl;
            _searchApiKey = searchApiKey;
        }

        public IndexedWordContainer GetOrCreateContainer(string containername) =>
            GetOrCreateContainer(containername, $"{containername}_index");      

        public IndexedWordContainer GetOrCreateContainer(string containername, string indexName) =>
           new  IndexedWordContainer(_blobConnectionString, _searchApiKey, _searchUrl, containername, indexName);

    }
}