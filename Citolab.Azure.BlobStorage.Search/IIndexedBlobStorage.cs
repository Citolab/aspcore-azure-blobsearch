namespace Citolab.Azure.BlobStorage.Search
{
    public interface IIndexedBlobStorage
    {
        bool ContainerExists(string containername);
        IndexedWordContainer GetOrCreateContainer(string containername);
        IndexedWordContainer GetOrCreateContainer(string containername, string indexName);
    }
}