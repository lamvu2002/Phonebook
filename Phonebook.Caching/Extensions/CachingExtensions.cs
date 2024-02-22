using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Metadata.Ecma335;
using Phonebook.Caching.Common;

namespace Phonebook.Caching.Extensions;

public static class CachingExtensions
{
    public static IServiceCollection AddCacheServices(this IServiceCollection services)
    {
        services.AddScoped<IDataCached, DataCached>();
        return services;
    }
    public static string GetKey<T>(this IDataCached dataCached, T? entity, Func<T, dynamic> acquire) where T : class
    => string.Format(CachingCommonDefaults.CacheKey, typeof(T).Name, acquire(entity!));
}
