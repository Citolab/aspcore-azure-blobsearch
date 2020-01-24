using System;
using Citolab.Azure.BlobStorage.Search;
using Citolab.Azure.BlobStorage.Search.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Citolab.Azure.BlobStorage.Search.Helpers;
using Microsoft.Azure.Search.Models;
using Index = Microsoft.Azure.Search.Models.Index;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(l =>
            {
                l.AddConsole();
            });
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            services.AddBlobStorage(new BlobSettings(
                configuration.GetValue<string>("BlobStorage:ConnectionString"),
                new Uri(configuration.GetValue<string>("BlobStorage:SearchUrl")),
                configuration.GetValue<string>("BlobStorage:ApiKey")));
            IServiceProvider provider = services.BuildServiceProvider();
            var storage = provider.GetService<IIndexedBlobStorage>();
            var logger = provider.GetService<ILoggerFactory>().CreateLogger("info");

            // 1. create container
            var container = storage.GetOrCreateContainer("algemeen");
            var indexName = $"{container.Name}-index";
           // 2.create blobs with documents
            Directory.GetFiles(@"C:\tmp\docs", "*.docx")
                .ToList()
                .ForEach(filename => container
                                    .GetBlockBlobReference(Path.GetFileNameWithoutExtension(filename))
                                    .GetOrCreateBlobByUploadingDocument(filename, false)
                                    .AddMetaData("subject", "math"));
            //3 add index
            var index = container.GetOrCreateIndex(new Index(indexName, new List<Field>())
                                                    .AddDefaultWordFields()
                                                    .AddField(new Field("subject", AnalyzerName.NlLucene)
                                                    {
                                                        Type = DataType.String,
                                                        IsFilterable = true
                                                    }));
            //4 add datasource + indexer
            container
                .CreateDatasourceIfNotExists($"{container.Name}-datasource", configuration.GetValue<string>("BlobStorage:ConnectionString"))
                .CreateIndexerIfNotExists($"{container.Name}-indexer", $"{container.Name}-datasource", index.Name, new List<FieldMapping>
                {
                    new FieldMapping("metadata_storage_path", FieldMappingFunction.Base64Encode()) //key cannot be an url therefore Encode it.
                }.ToArray());
            //5. Search for words within a subject.
            var searchResult = container.Search(indexName, "mayonaise", new Filter("subject", FilterOperator.eq, "math"));
            searchResult.ForEach(s => logger.LogInformation($"result found: {s.ToString()}"));

        }
    }
}
