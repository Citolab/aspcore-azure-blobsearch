using System;
using Microsoft.Extensions.DependencyInjection;

namespace Citolab.Azure.BlobStorage.Search.Extensions
{
    public static class Extensions
    {
        public static IServiceCollection AddBlobStorage(this IServiceCollection services, BlobSettings settings)
        {
            var indexBlobStorage = new IndexedBlobStorage(settings.BlobConnectionString, settings.SearchServiceName, settings.SearchApiKey);
            services.AddSingleton<IIndexedBlobStorage>(indexBlobStorage);
            return services;
        }
    }
}
