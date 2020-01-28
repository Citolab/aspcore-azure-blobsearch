using Microsoft.Azure.Search.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.Search;
using System.Threading;

namespace Citolab.Azure.BlobStorage.Search.Helpers
{
    public static class IndexedWordContainerExtensions
    {
        public static async Task<Index> GetOrCreateIndex(this IndexedWordContainer container, Index index)
        {
            if ( await container.SearchServiceClient.Indexes.ExistsAsync(index.Name))
            {
                return await container.SearchServiceClient.Indexes.GetAsync(index.Name);
            }
            return await container.SearchServiceClient.Indexes.CreateOrUpdateAsync(index);
        }

        public static async Task<IndexedWordContainer> CreateDatasourceIfNotExists(this IndexedWordContainer container, string name, string connectionstring)
        {
            var _ = await container.SearchServiceClient.DataSources.ExistsAsync(name) ?
                await container.SearchServiceClient.DataSources.GetAsync(name) :
                await container.SearchServiceClient.DataSources.CreateOrUpdateAsync(DataSource.AzureBlobStorage(name, connectionstring, container.Name));
            return container;
        }
        public static async Task<Indexer> CreateIndexerIfNotExists(this IndexedWordContainer container, string name, string datasourceName, string indexName, FieldMapping[] mapping) =>
            await container.SearchServiceClient.Indexers.ExistsAsync(name) ?
                await container.SearchServiceClient.Indexers.GetAsync(name) : 
                Task.Run(() =>
                {
                    var indexer = container.SearchServiceClient.Indexers.CreateOrUpdate(new Indexer(name, datasourceName, indexName,
                        fieldMappings: new List<FieldMapping>
                        {
                            new FieldMapping("metadata_storage_path", FieldMappingFunction.Base64Encode()) //key cannot be an url therefore Encode it.
                        }));
                    Thread.Sleep(1000);
                    container.SearchServiceClient.Indexers.Run(name);
                    Thread.Sleep(1000);
                    return indexer;
                }).Result;

        public static async Task<IndexedWordContainer> RebuildIndexes(this IndexedWordContainer container)
        {
            var list = await container.SearchServiceClient.Indexers.ListAsync();
            list.Indexers.ToList().ForEach(async i =>
            {
                try
                {
                    await container.SearchServiceClient.Indexers.RunAsync(i.Name);
                }
                catch
                { // do nothing;
                }

            });
            return container;
        }

        public static async Task<Indexer> AddFieldMapping(this Indexer indexer, IndexedWordContainer container, FieldMapping mapping)
        {
            if (indexer.FieldMappings.Any(f => f.SourceFieldName == mapping.SourceFieldName)) return indexer;
            indexer.FieldMappings.Add(mapping);
            return await container.SearchServiceClient.Indexers.CreateOrUpdateAsync(indexer);
        }
    }
}
