﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Citolab.Azure.BlobStorage.Search.Helpers
{
    public static class CloudHelper
    {
        public static CloudBlobContainer GetCloudBlobContainer(string connectionString, string containerName) =>
            GetCloudBlobContainer(GetCloudBlobClient(connectionString), containerName);

        public static CloudBlobContainer GetCloudBlobContainer(CloudBlobClient client, string containerName)
        {
            var container = client.GetContainerReference(containerName);
            var _ = container.CreateIfNotExistsAsync().Result;
            return container;
        }

        public static CloudBlobClient GetCloudBlobClient(string connectionString) =>
            CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();
    }
}
