# Citolab.Azure.BlobStorage.Documents.Search

This is a repository library that helps uploading and searching word documents in Azure Blob Storage

## Prerequisites

1. Storage account with 'Account kind': Blob storage.
2. Search service - if you want to search in the content of the documents

## Usage


```C#
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
```
