namespace Phonebook.Caching.Common;

public static partial class CachingCommonDefaults
{
    public static int CacheTime = 15;

    public static string CacheKey => "solid.ecommerce.{0}.id.{1}";
    public static string AllCacheKey => "solid.ecommerce.{0}.all";
    public static string CacheKeyHeader => "solid.ecommerce.{0}";
}
