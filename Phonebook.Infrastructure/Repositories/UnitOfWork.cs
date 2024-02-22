namespace Phonebook.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private Dictionary<string, object> Repositories { get; }
    private IDbContextTransaction _transaction;
    private IsolationLevel? _isolationLevel;
    public IApplicationDbContext ApplicationDbContext { get; private set; }

    public UnitOfWork(IApplicationDbContext applicationDbContext)
    {
        ApplicationDbContext = applicationDbContext;
        Repositories = new Dictionary<string, dynamic>();
    }

    public async Task BeginTransactionAsync()
    {
        await StartNewTransactionIfNeeded();
    }


    public async Task CommitTransactionAsync()
    {
        await ApplicationDbContext.DbContext.SaveChangesAsync();
        if (_transaction is null) return;
        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }
    /// <summary>
    /// Return a instance of Repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        var typeName = type.Name;
        lock (Repositories)
        {
            if (Repositories.ContainsKey(typeName)) return (IRepository<T>)Repositories[typeName];

            var repository = new Repository<T>(ApplicationDbContext);

            Repositories.Add(typeName, repository);

            return repository;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null) return;
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await ApplicationDbContext.DbContext.SaveChangesAsync(cancellationToken);
    /// <summary>
    /// Beign a transaction
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task StartNewTransactionIfNeeded()
    {
        if (_transaction == null)
        {
            _transaction = _isolationLevel.HasValue ?
                await ApplicationDbContext.DbContext.Database.BeginTransactionAsync(_isolationLevel.GetValueOrDefault()) :
                await ApplicationDbContext.DbContext.Database.BeginTransactionAsync();
        }
    }
    public void Dispose()
    {
        if (ApplicationDbContext == null) return;
        if (ApplicationDbContext.DbContext.Database.GetDbConnection().State == ConnectionState.Open)
            ApplicationDbContext.DbContext.Database.GetDbConnection().Close();
        ApplicationDbContext.DbContext.Dispose();
        ApplicationDbContext = null;
        GC.SuppressFinalize(this);
    }
}
