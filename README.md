# Citolab Azure Blob Storage Helper

This is a repository library that helps uploading and searching word documents in Azure Blob Storage

## Prerequisites

1. Storage account with 'Account kind': Blob storage.
2. Search service - if you want to search in the content of the documents

The constructor of the SearchableWordBlob needs an index name. If the index doesn't exist it's going to create a datasource, index and indexer the first time you perform a search. I you want to have more control in the way the index is created you can create te index yourself and pass the index name.

## Usage


```C#
// Upload document 1 using WordBlob 
var wordBlob = new WordBlob("DefaultEndpointsProtocol=https;AccountName=**;AccountKey=**;EndpointSuffix=core.windows.net", "**");
var container1 = wordBlob.AddDocument("C:\\doc1.docx").Result;

// Upload document 2 and search through documents using SearchableWordBlob 
var searchableWordBlob = new SearchableWordBlob(
    "DefaultEndpointsProtocol=https;AccountName=**;AccountKey=**;EndpointSuffix=core.windows.net", "**", 
    new Uri("https://**.search.windows.net"), "opgavenindex", "2AC8D6AD54A5D8F8277C91CFC5406C28");
var container2 = searchableWordBlob.AddDocument("C:\\doc2.docx").Result;
var uris = searchableWordBlob.Search("Keyword");
```