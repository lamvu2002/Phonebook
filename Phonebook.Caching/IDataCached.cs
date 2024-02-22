namespace Phonebook.Caching;

public interface IDataCached
{
    void Set<T>(string key, T? value, int cacheTime);
    bool IsSet(string key);
    T Get<T>(string key, Func<T> acquire, int? cacheTime = null);
    T GetT<T>(string key);
    object Get(string key);
    IList<string> GetKeys();
    IList<T> GetValues<T>(string pattern);
    void Remove(string key);
    void Clear();
}
