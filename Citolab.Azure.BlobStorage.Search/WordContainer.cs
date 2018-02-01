using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Citolab.Azure.BlobStorage.Search.Helpers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Citolab.Azure.BlobStorage.Search
{
    public class WordContainer
    {
        protected readonly CloudBlobContainer _cloudBlobContainer;
        protected readonly string _connectionString;
        protected readonly string _containerName;

        public WordContainer(string connectionString, string containerName)
        {
            _containerName = containerName;
            _connectionString = connectionString;
            _cloudBlobContainer = CloudHelper.GetCloudBlobContainer(_connectionString, _containerName);
        }

        public Task<string> AddDocument(string filePath, bool overwrite = false) =>
            _cloudBlobContainer
                .GetBlockBlobReference(Path.GetFileName(filePath))
                .UploadDocument(filePath, overwrite);

        public Task<string> AddDocument(Stream stream, string blobName, bool overwrite = false) =>
            _cloudBlobContainer
                .GetBlockBlobReference(blobName)
                .UploadDocument(stream, overwrite);

        public string GetDocumentUrl(string containerName) =>
            _cloudBlobContainer.Uri.ToString().AddToken(_cloudBlobContainer);

        public IEnumerable<string> GetDocumentUrls(IEnumerable<string> containerNames) =>
            containerNames.Select(GetDocumentUrl);

    }


}
