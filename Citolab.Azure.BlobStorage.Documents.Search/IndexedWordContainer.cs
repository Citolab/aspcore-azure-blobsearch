using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Citolab.Azure.BlobStorage.Search.Helpers;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.Storage.Blob;

namespace Citolab.Azure.BlobStorage.Search
{
    public class IndexedWordContainer 
    {
        public string Name => BaseContainer.Name;

        public SearchServiceClient SearchServiceClient { get; set; }
        public CloudBlobContainer BaseContainer;
        //private readonly Uri _searchUrl;
        //private readonly string _indexName;
        //private readonly string _searchApiKey;

        public IndexedWordContainer(string connectionString, string containerName, string searchServiceName, string searchApiKey)
        {
            SearchServiceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(searchApiKey));
            BaseContainer = CloudHelper.GetCloudBlobContainer(connectionString, containerName);
        }

        public async Task<Uri> UploadDocument(Stream stream, string fileName, bool overwrite = false)
        {
            var block = await 
                GetBlockBlobReference(Path.GetFileNameWithoutExtension(fileName))
                .GetOrCreateBlobByUploadingDocument(stream, overwrite, Path.GetExtension(fileName));
            return block?.Uri;
        }

        public async Task<Uri> UploadDocument(string filePath, bool overwrite = false, List<KeyValuePair<string, string>> metaData = null)
        {
            var block = await
                GetBlockBlobReference(Path.GetFileNameWithoutExtension(filePath))
                    .GetOrCreateBlobByUploadingDocument(filePath, overwrite);
            metaData?.ForEach(async data =>
            {
                await block.AddMetaData(data.Key, data.Value);
            });
            return block?.Uri;
        }

        public CloudBlockBlob GetBlockBlobReference(string name) =>
            BaseContainer.GetBlockBlobReference(name);

        public List<Uri> Search(string indexName, string keyword) =>
                GetSearchServiceClient(indexName)
                .Documents
                .Search(keyword)
                .Results.Select(r =>
                    r.Document["metadata_storage_path"]
                        .ToString()
                        .DecodeStoragePath()
                        .AddToken(BaseContainer))
                .ConvertToUri()
                .ToList();

        public Uri Get(string name) =>
              Uri.TryCreate(BaseContainer.GetBlockBlobReference(name)
                .Uri.ToString().AddToken(BaseContainer), UriKind.Absolute, out var result) ? 
                    result: null;
            
        public List<Uri> Search(string indexName, string keyword, Filter filter) =>
                GetSearchServiceClient(indexName)
                .Documents
                .Search(keyword, new SearchParameters { Filter = $"{filter.FieldName} {filter.FilterOperator.ToString()} '{filter.Value}'" })
                .Results.Select(r =>
                    r.Document["metadata_storage_path"]
                        .ToString()
                        .DecodeStoragePath()
                        .AddToken(BaseContainer))
                .ConvertToUri()
                .ToList();

        private ISearchIndexClient GetSearchServiceClient(string indexName) =>
            SearchServiceClient.Indexes.Exists(indexName) ? SearchServiceClient.Indexes.GetClient(indexName) : null;

    }
}
