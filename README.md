# Citolab.Azure.BlobStorage.Search

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
                                            .AddField(new Field() { Name = "subject", Type = DataType.String, IsFilterable = true }));
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
```

The urls that are returned can be used for 7 days.