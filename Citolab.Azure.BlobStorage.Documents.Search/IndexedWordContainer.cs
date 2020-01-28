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
        private string _storageConnectionString = "";

        public IndexedWordContainer(string connectionString, string containerName, string searchServiceName, string searchApiKey)
        {
            _storageConnectionString = connectionString;
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
        
        public async Task<Microsoft.Azure.Search.Models.Index> GetOrCreateIndex(Microsoft.Azure.Search.Models.Index index)
        {
            if (await SearchServiceClient.Indexes.ExistsAsync(index.Name))
            {
                return await SearchServiceClient.Indexes.GetAsync(index.Name);
            }
            return await SearchServiceClient.Indexes.CreateOrUpdateAsync(index);
        }

        public async Task<IndexedWordContainer> CreateDatasourceIfNotExists(string name)
        {
            if (await SearchServiceClient.DataSources.ExistsAsync(name))
            {
                await SearchServiceClient.DataSources.GetAsync(name);
            }
            else
            {
                await SearchServiceClient.DataSources.CreateOrUpdateAsync(
                    DataSource.AzureBlobStorage(name, _storageConnectionString, Name));
            }
            return this;
        }
        public  async Task<Indexer> CreateIndexerIfNotExists(string name, string datasourceName, string indexName, FieldMapping[] mapping) =>
            await SearchServiceClient.Indexers.ExistsAsync(name) ?
                await SearchServiceClient.Indexers.GetAsync(name) :
                Task.Run(() =>
                {
                    var indexer = SearchServiceClient.Indexers.CreateOrUpdate(new Indexer(name, datasourceName, indexName,
                        fieldMappings: new List<FieldMapping>
                        {
                            new FieldMapping("metadata_storage_path", FieldMappingFunction.Base64Encode()) //key cannot be an url therefore Encode it.
                        }));
                    Thread.Sleep(1000);
                    SearchServiceClient.Indexers.Run(name);
                    Thread.Sleep(1000);
                    return indexer;
                }).Result;

        public async Task<IndexedWordContainer> RebuildIndexes()
        {
            var list = await SearchServiceClient.Indexers.ListAsync();
            list.Indexers.ToList().ForEach(async i =>
            {
                try
                {
                    await SearchServiceClient.Indexers.RunAsync(i.Name);
                }
                catch
                { // do nothing;
                }

            });
            return this;
        }


        private ISearchIndexClient GetSearchServiceClient(string indexName) =>
            SearchServiceClient.Indexes.Exists(indexName) ? SearchServiceClient.Indexes.GetClient(indexName) : null;

    }
}
