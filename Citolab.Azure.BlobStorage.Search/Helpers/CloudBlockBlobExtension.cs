using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Citolab.Azure.BlobStorage.Search.Helpers
{
    public static class CloudBlockBlobExtension
    {
        public static CloudBlockBlob GetOrCreateBlobByUploadingDocument(this CloudBlockBlob blockBlob, string filePath, bool overwrite)
        {
            if (blockBlob.ExistsAsync().Result && !overwrite) return blockBlob;
            blockBlob.UploadFromFileAsync(filePath).Wait();
            return blockBlob;
        }

        public static async Task<CloudBlockBlob> GetOrCreateBlobByUploadingDocument(this CloudBlockBlob blockBlob, Stream stream, bool overwrite)
        {
            if (blockBlob.ExistsAsync().Result && !overwrite) return await Task.FromResult(blockBlob);
            var taskCompletion = new TaskCompletionSource<CloudBlockBlob>();
            var _ = blockBlob.UploadFromStreamAsync(stream).ContinueWith(result =>
                taskCompletion.SetResult(blockBlob));
            return await taskCompletion.Task;
        }
        
        public static CloudBlob AddMetaData(this CloudBlockBlob blob, string key, string value)
        {
            if (!blob.Metadata.ContainsKey(key)) {
                blob.Metadata.Add(new KeyValuePair<string, string>(key, value));
                blob.SetMetadataAsync().Wait();
            }
            return blob;
        }
    }
}
