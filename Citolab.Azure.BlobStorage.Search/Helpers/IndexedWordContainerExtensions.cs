using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.Search;
using System.Threading;

namespace Citolab.Azure.BlobStorage.Search.Helpers
{
    public static class IndexedWordContainerExtensions
    {
        public static Index GetOrCreateIndex(this IndexedWordContainer container, Index index) =>
            container.SearchServiceClient.Indexes.Exists(index.Name) ?
                container.SearchServiceClient.Indexes.Get(index.Name) :
                container.SearchServiceClient.Indexes.CreateOrUpdate(index);


        public static IndexedWordContainer CreateDatasourceIfNotExists(this IndexedWordContainer container, string name, string connectionstring)
        {
            var _ = container.SearchServiceClient.DataSources.Exists(name) ?
                container.SearchServiceClient.DataSources.Get(name) :
                container.SearchServiceClient.DataSources.CreateOrUpdate(DataSource.AzureBlobStorage(name, connectionstring, container.Name));
            return container;
        }
        public static Indexer CreateIndexerIfNotExists(this IndexedWordContainer container, string name, string datasourceName, string indexName, FieldMapping[] mapping) =>
            container.SearchServiceClient.Indexers.Exists(name) ?
                container.SearchServiceClient.Indexers.Get(name) :
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

        public static IndexedWordContainer RebuildIndexes(this IndexedWordContainer container)
        {
            var list = container.SearchServiceClient.Indexers.List();
            list.Indexers.ToList().ForEach(i =>
            {
                try
                {
                    container.SearchServiceClient.Indexers.Run(i.Name);
                }
                catch
                { // do nothing;
                }

            });
            return container;
        }

        public static Indexer AddFieldMapping(this Indexer indexer, IndexedWordContainer container, FieldMapping mapping)
        {
            if (!indexer.FieldMappings.Any(f => f.SourceFieldName == mapping.SourceFieldName))
            {
                indexer.FieldMappings.Add(mapping);
                return container.SearchServiceClient.Indexers.CreateOrUpdate(indexer);
            }
            return indexer;
        }
    }
}
