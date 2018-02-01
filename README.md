# Citolab.Azure.BlobStorage.Search

This is a repository library that helps uploading and searching word documents in Azure Blob Storage

## Prerequisites

1. Storage account with 'Account kind': Blob storage.
2. Search service - if you want to search in the content of the documents

The constructor of the SearchableWordBlob needs an index name. If the index doesn't exist it's going to create a datasource, index and indexer the first time you perform a search. I you want to have more control in the way the index is created you can create te index yourself and pass the index name.

## Usage


```C#
// Upload document 1 using WordContainer 
var wordContainer = new WordContainer("DefaultEndpointsProtocol=https;AccountName=**;AccountKey=**;EndpointSuffix=core.windows.net", "**");
var blob1 = wordContainer.AddDocument("C:\\doc1.docx").Result;
var documentUrl = wordContainer.GetDocumentUrl(blob1);

// Upload document 2 and search through documents using IndexedWordContainer 
var indexedWordContainer = new IndexedWordContainer(
    "DefaultEndpointsProtocol=https;AccountName=**;AccountKey=**;EndpointSuffix=core.windows.net", "**", 
    new Uri("https://**.search.windows.net"), "opgavenindex", "2AC8D6AD54A5D8F8277C91CFC5406C28");
var blob2 = indexedWordContainer.AddDocument("C:\\doc2.docx").Result;
var uris = indexedWordContainer.Search("Keyword");
```

The urls that are returned can be used for 7 days.