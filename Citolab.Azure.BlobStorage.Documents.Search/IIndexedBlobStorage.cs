namespace Citolab.Azure.BlobStorage.Search
{
    public interface IIndexedBlobStorage
    {
        IndexedWordContainer GetOrCreateContainer(string containername);
    }
}