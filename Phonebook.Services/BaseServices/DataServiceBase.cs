namespace Phonebook.Services.BaseServices;

public abstract class DataServiceBase<TEntity> : IDataService<TEntity> where TEntity : class, new()
{
    protected IUnitOfWork UnitOfWork { get; private set; }

    protected DataServiceBase(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    public abstract Task<IList<TEntity>> GetAllAsync();
    public abstract Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
    public abstract Task<TEntity> GetOneAsync(int? id);
    public abstract Task UpdateAsync(TEntity entity);
    public abstract Task AddAsync(TEntity entity);
    public abstract Task DeleteAsync(int? id);
    public abstract Task DeleteAsync(TEntity entity);
}
