namespace Phonebook.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    IApplicationDbContext ApplicationDbContext { get; }
    /// <summary>
    /// Get repository instance of an entity (T) inside repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
