using Microsoft.EntityFrameworkCore;
using Phonebook.Shared;

namespace Phonebook.Infrastructure.Context;
/// <summary>
/// Implement design pattern factory to create a object related database operations
/// </summary>
public class DbFactoryContext
{
    private DbContext _dbContext;
    private Func<PhonebookContext> _instanceFunc;
    public DbContext DbContext => _dbContext ?? (_dbContext = _instanceFunc.Invoke());
    public DbFactoryContext(Func<PhonebookContext> dbContextFactory)
    {
        _instanceFunc = dbContextFactory;
    }
}
