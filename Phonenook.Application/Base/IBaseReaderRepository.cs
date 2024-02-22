namespace Phonebook.Application.Base;

public interface IBaseReaderRepository<T> : IBaseRepo<T> where T : class
{
    /// <summary>
    /// Get all items of an entity by asynchronous
    /// </summary>
    /// <returns> List of T</returns>
    Task<IList<T>> GetAllAsync();
    T Find(params object[] keyValues);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyValues"></param>
    /// <returns></returns>
    Task<T> FindAsync(params object[] keyValues);
}
