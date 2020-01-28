using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static string AddToken(this string url, CloudBlobContainer container,
            DateTimeOffset sharedAccessExpiryTime, DateTimeOffset sharedAccessStartTime) =>
            string.Concat(url, container.SharedAccessBlobPolicy(sharedAccessExpiryTime, sharedAccessStartTime));

        public static string AddToken(this string url, CloudBlobContainer container) =>
            string.Concat(url,
                container.SharedAccessBlobPolicy(DateTimeOffset.UtcNow.AddDays(7), DateTimeOffset.UtcNow.AddDays(-1)));

        public static IEnumerable<Uri> ConvertToUri(this IEnumerable<string> list) =>
            list.Select(uriString => Uri.TryCreate(uriString, UriKind.Absolute, out var uri) ? uri : null)
                .Where(u => u != null);

        private static string SharedAccessBlobPolicy(this CloudBlobContainer container,
            DateTimeOffset sharedAccessExpiryTime, DateTimeOffset sharedAccessStartTime) =>
            container.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = sharedAccessExpiryTime,
                SharedAccessStartTime = sharedAccessStartTime,
                Permissions = SharedAccessBlobPermissions.Read
            });

        public static string GetMimeType(this string extension )
        {
            switch (extension)
            {
                case ".7z": return "application/x-7z-compressed";
                case ".ac3": return "audio/ac3";
                case ".aca": return "application/octet-stream";
                case ".avi": return "video/x-msvideo";
                case ".bin": return "application/octet-stream";
                case ".bmp": return "image/bmp";
                case ".css": return "text/css";
                case ".csv": return "text/csv";
                case ".dll": return "application/x-msdownload";
                case ".dll.config": return "text/xml";
                case ".dlm": return "text/dlm";
                case ".doc": return "application/msword";
                case ".docm": return "application/vnd.ms-word.document.macroEnabled.12";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".dot": return "application/msword";
                case ".dotm": return "application/vnd.ms-word.template.macroEnabled.12";
                case ".dotx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.template";
                case ".eot": return "application/octet-stream";
                case ".eps": return "application/postscript";
                case ".exe": return "application/octet-stream";
                case ".exe.config": return "text/xml";
                case ".gif": return "image/gif";
                case ".gz": return "application/x-gzip";
                case ".htm": return "text/html";
                case ".html": return "text/html";
                case ".ico": return "image/x-icon";
                case ".jpe": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".jpg": return "image/jpeg";
                case ".js": return "application/x-javascript";
                case ".json": return "application/json";
                case ".latex": return "application/x-latex";
                case ".m3u": return "audio/x-mpegurl";
                case ".m3u8": return "audio/x-mpegurl";
                case ".m4a": return "audio/m4a";
                case ".m4b": return "audio/m4b";
                case ".m4p": return "audio/m4p";
                case ".m4r": return "audio/x-m4r";
                case ".m4v": return "video/x-m4v";
                case ".mac": return "image/x-macpaint";
                case ".manifest": return "application/x-ms-manifest";
                case ".mht": return "message/rfc822";
                case ".mhtml": return "message/rfc822";
                case ".mid": return "audio/mid";
                case ".midi": return "audio/mid";
                case ".mod": return "video/mpeg";
                case ".mov": return "video/quicktime";
                case ".movie": return "video/x-sgi-movie";
                case ".mp2": return "video/mpeg";
                case ".mp2v": return "video/mpeg";
                case ".mp3": return "audio/mpeg";
                case ".mp4": return "video/mp4";
                case ".mp4v": return "video/mp4";
                case ".mpa": return "video/mpeg";
                case ".mpe": return "video/mpeg";
                case ".mpeg": return "video/mpeg";
                case ".mpf": return "application/vnd.ms-mediapackage";
                case ".mpg": return "video/mpeg";
                case ".mpp": return "application/vnd.ms-project";
                case ".mpv2": return "video/mpeg";
                case ".mqv": return "video/quicktime";
                case ".msi": return "application/octet-stream";
                case ".ocx": return "application/octet-stream";
                case ".pdf": return "application/pdf";
                case ".pic": return "image/pict";
                case ".pict": return "image/pict";
                case ".pls": return "audio/scpls";
                case ".png": return "image/png";
                case ".pnm": return "image/x-portable-anymap";
                case ".pnt": return "image/x-macpaint";
                case ".pntg": return "image/x-macpaint";
                case ".pnz": return "image/png";
                case ".pot": return "application/vnd.ms-powerpoint";
                case ".potm": return "application/vnd.ms-powerpoint.template.macroEnabled.12";
                case ".potx": return "application/vnd.openxmlformats-officedocument.presentationml.template";
                case ".ppa": return "application/vnd.ms-powerpoint";
                case ".ppam": return "application/vnd.ms-powerpoint.addin.macroEnabled.12";
                case ".ppm": return "image/x-portable-pixmap";
                case ".pps": return "application/vnd.ms-powerpoint";
                case ".ppsm": return "application/vnd.ms-powerpoint.slideshow.macroEnabled.12";
                case ".ppsx": return "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                case ".ppt": return "application/vnd.ms-powerpoint";
                case ".pptm": return "application/vnd.ms-powerpoint.presentation.macroEnabled.12";
                case ".pptx": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".prf": return "application/pics-rules";
                case ".psd": return "application/octet-stream";
                case ".rar": return "application/octet-stream";
                case ".swf": return "application/x-shockwave-flash";
                case ".tif": return "image/tiff";
                case ".tiff": return "image/tiff";
                case ".txt": return "text/plain";
                case ".vsd": return "application/vnd.visio";
                case ".wav": return "audio/wav";
                case ".wave": return "audio/wav";
                case ".webarchive": return "application/x-safari-webarchive";
                case ".wm": return "video/x-ms-wm";
                case ".wma": return "audio/x-ms-wma";
                case ".wmd": return "application/x-ms-wmd";
                case ".wmf": return "application/x-msmetafile";
                case ".wml": return "text/vnd.wap.wml";
                case ".wmp": return "video/x-ms-wmp";
                case ".wmv": return "video/x-ms-wmv";
                case ".wmx": return "video/x-ms-wmx";
                case ".xaml": return "application/xaml+xml";
                case ".xla": return "application/vnd.ms-excel";
                case ".xlam": return "application/vnd.ms-excel.addin.macroEnabled.12";
                case ".xlc": return "application/vnd.ms-excel";
                case ".xld": return "application/vnd.ms-excel";
                case ".xlk": return "application/vnd.ms-excel";
                case ".xll": return "application/vnd.ms-excel";
                case ".xlm": return "application/vnd.ms-excel";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlsb": return "application/vnd.ms-excel.sheet.binary.macroEnabled.12";
                case ".xlsm": return "application/vnd.ms-excel.sheet.macroEnabled.12";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".xlt": return "application/vnd.ms-excel";
                case ".xltm": return "application/vnd.ms-excel.template.macroEnabled.12";
                case ".xltx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.template";
                case ".xlw": return "application/vnd.ms-excel";
                case ".xml": return "text/xml";
                case ".XOML": return "text/plain";
                case ".xsd": return "text/xml";
                case ".xsf": return "text/xml";
                case ".xsl": return "text/xml";
                case ".xslt": return "text/xml";
                case ".xss": return "application/xml";
                case ".zip": return "application/x-zip-compressed";
            }

            return string.Empty;
        }

    }
}
