﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Citolab.Azure.BlobStorage.Search.Helpers;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace Citolab.Azure.BlobStorage.Search
{
    public class IndexedWordContainer : WordContainer
    {


        private readonly Uri _searchUrl;
        private readonly string _indexName;
        private readonly string _searchApiKey;

        public IndexedWordContainer(string blobConnectionString, string containerName, Uri searchUrl,  string indexName, string searchApiKey) : base(blobConnectionString, containerName)
        {
            _searchUrl = searchUrl;
            _indexName = indexName;
            _searchApiKey = searchApiKey;
        }
               

        public List<Uri> Search(string keyword) =>
            GetSearchIndexClient()
                .Documents
                .Search(keyword)
                .Results.Select(r =>
                    r.Document["metadata_storage_path"]
                        .ToString()
                        .DecodeStoragePath()
                        .AddToken(_cloudBlobContainer))
                .ConvertToUri()
                .ToList();

        private SearchServiceClient GetServiceClient() =>
            new SearchServiceClient(_searchUrl, new SearchCredentials(_searchApiKey));

        /// <summary>
        /// This function gets the SearchIndexClient by index name.
        /// If the index does not exist it's going to create a datasource, index and indexer.
        /// </summary>
        /// <returns></returns>
        private ISearchIndexClient GetSearchIndexClient()
        {
            var searchService = GetServiceClient();
            var searchIndexClient = searchService.Indexes.Exists(_indexName) ? searchService.Indexes.GetClient(_indexName) : null;
            if (searchIndexClient != null) return searchIndexClient;
            var datasource = $"{_containerName}_wordblob_datasource";
            var indexer = $"{_containerName}_wordblob_indexer";
            searchService.DataSources
                .CreateOrUpdate(datasource, DataSource.AzureBlobStorage(datasource, _connectionString, _cloudBlobContainer.Name));
            searchService.Indexes.CreateOrUpdate(_indexName, new Index(_indexName,
                new List<Field>
                {
                    new Field() {Name = "content", Type = DataType.String, IsRetrievable = true, IsSearchable = true},
                    new Field() {Name = "metadata_storage_path",Type = DataType.String,  IsKey = true, IsRetrievable = true}
                }));
            searchService.Indexers.CreateOrUpdate(new Indexer(indexer, datasource, _indexName,
                fieldMappings: new List<FieldMapping>
                {
                    new FieldMapping("metadata_storage_path", FieldMappingFunction.Base64Encode()) //key cannot be an url therefore Encode it.
                }));
            Thread.Sleep(2000); //Wait till index is build
            searchIndexClient = searchService.Indexes.GetClient(_indexName);
            return searchIndexClient;
        }

    }
}
