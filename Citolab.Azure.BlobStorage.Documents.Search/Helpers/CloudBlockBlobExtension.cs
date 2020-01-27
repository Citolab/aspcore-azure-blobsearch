using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace Citolab.Azure.BlobStorage.Search.Helpers
{
    public static class CloudBlockBlobExtension
    {
        public static async Task<CloudBlockBlob> GetOrCreateBlobByUploadingDocument(this CloudBlockBlob blockBlob, string filePath, bool overwrite)
        {
            if (blockBlob.ExistsAsync().Result && !overwrite) return blockBlob;
            await blockBlob.UploadFromFileAsync(filePath);
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
        
        public static async Task<CloudBlob> AddMetaData(this CloudBlockBlob blob, string key, string value)
        {
            if (!blob.Metadata.ContainsKey(key)) {
                blob.Metadata.Add(new KeyValuePair<string, string>(key, value));
                await blob.SetMetadataAsync();
            }
            return blob;
        }
    }
}
