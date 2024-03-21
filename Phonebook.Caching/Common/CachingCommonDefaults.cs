namespace Phonebook.Caching.Common;

public static partial class CachingCommonDefaults
{
    public static int CacheTime = 5;

    public static string CacheKey => "phonebook.{0}.id.{1}";
    public static string AllCacheKey => "phonebook.{0}.all";
    public static string CacheKeyHeader => "phonebook.{0}";
}
