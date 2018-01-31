using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Citolab.Azure.BlobStorage.Search
{
    public class WordBlob
    {
        protected readonly CloudBlobContainer _cloudBlobContainer;
        protected readonly string _connectionString;
        protected readonly string _container;

        public WordBlob(string connectionString, string container)
        {
            _container = container;
            _connectionString = connectionString;
            _cloudBlobContainer = GetCloudBlobContainer();
        }

        public Task<string> AddDocument(string filePath, bool overwrite = false) =>
            _cloudBlobContainer
                .GetBlockBlobReference(Path.GetFileName(filePath))
                .UploadDocument(filePath, overwrite);

        public Task<string> AddDocument(Stream stream, string blobName, bool overwrite = false) =>
            _cloudBlobContainer
                .GetBlockBlobReference(blobName)
                .UploadDocument(stream, overwrite);
       

        private CloudBlobContainer GetCloudBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_container);
            var _ = container.CreateIfNotExistsAsync().Result;
            return container;
        }
    }


}
