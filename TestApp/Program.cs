using System;
using Citolab.Azure.BlobStorage.Search;
using Citolab.Azure.BlobStorage.Search.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Citolab.Azure.BlobStorage.Search.Helpers;
using Microsoft.Azure.Search.Models;
using Index = Microsoft.Azure.Search.Models.Index;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(l =>
            {
                l.AddConsole();
            });
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true) //load base settings
                .AddJsonFile("appsettings.local.json", true, true); //load local settings
            var configuration = builder.Build();
            services.AddBlobStorage(new BlobSettings(
                configuration.GetValue<string>("BlobStorage:ConnectionString"),
                configuration.GetValue<string>("BlobStorage:SearchServiceName"),
                configuration.GetValue<string>("BlobStorage:ApiKey")));
            IServiceProvider provider = services.BuildServiceProvider();
            var storage = provider.GetService<IIndexedBlobStorage>();
            var logger = provider.GetService<ILoggerFactory>().CreateLogger("info");

            // 1. create container
            var container = storage.GetOrCreateContainer(configuration.GetValue<string>("BlobStorage:Container"));
            var indexName = $"{container.Name}-index";
            // 2.create blobs with documents
            new DirectoryInfo(configuration.GetValue<string>("AppSettings:DocumentFolder"))
                .GetFiles("*.*", SearchOption.AllDirectories)
                .Where(d => d.Extension == ".docx" || d.Extension == ".doc" || d.Extension == ".pdf")
                .ToList()
                .ForEach(async filename =>
                {
                    await container.UploadDocument(filename.FullName, metaData: new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("subject", "math")
                    });
                });
            //3 add index
            var index = await container.GetOrCreateIndex(new Index(indexName, new List<Field>())
                .AddDefaultWordFields("nl")
                .AddField(new Field("subject", AnalyzerName.NlLucene)
                {
                    Type = DataType.String,
                    IsFilterable = true
                })
             );
            //4 add datasource + indexer
            var datasource = await container
                .CreateDatasourceIfNotExists($"{container.Name}-datasource");

            await datasource.CreateIndexerIfNotExists($"{container.Name}-indexer", $"{container.Name}-datasource", index.Name, new List<FieldMapping>
                {
                    new FieldMapping("metadata_storage_path", FieldMappingFunction.Base64Encode()) //key cannot be an url therefore Encode it.
                }.ToArray());
            //5. Search for words within a subject.
            var searchResult = container.Search(indexName, "Something to search for");
            searchResult.ForEach(s => logger.LogInformation($"result found: {s}"));

        }
    }
}
