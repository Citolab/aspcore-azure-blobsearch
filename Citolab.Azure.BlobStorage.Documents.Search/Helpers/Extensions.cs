﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Storage.Blob;

namespace Citolab.Azure.BlobStorage.Search.Helpers
{
    public static class Extensions
    {
        public static string DecodeStoragePath(this string path) =>
            !string.IsNullOrEmpty(path)
                ? Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(path.Substring(0, path.Length - 1)))
                : string.Empty;

        public static string UrlEncode(this string path) =>
            WebUtility.UrlEncode(path);

        public static string AddToken(this string url, CloudBlobContainer container, DateTimeOffset sharedAccessExpiryTime, DateTimeOffset sharedAccessStartTime) =>
            string.Concat(url, container.SharedAccessBlobPolicy(sharedAccessExpiryTime, sharedAccessStartTime));

        public static string AddToken(this string url, CloudBlobContainer container) =>
            string.Concat(url, container.SharedAccessBlobPolicy(DateTimeOffset.UtcNow.AddDays(7), DateTimeOffset.UtcNow.AddDays(-1)));

        public static IEnumerable<Uri> ConvertToUri(this IEnumerable<string> list) =>
            list.Select(uriString => Uri.TryCreate(uriString, UriKind.Absolute, out var uri) ? uri : null)
                .Where(u => u != null);

        private static string SharedAccessBlobPolicy(this CloudBlobContainer container, DateTimeOffset sharedAccessExpiryTime, DateTimeOffset sharedAccessStartTime) =>
            container.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = sharedAccessExpiryTime,
                SharedAccessStartTime = sharedAccessStartTime,
                Permissions = SharedAccessBlobPermissions.Read
            });
    }
}
