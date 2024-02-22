using Microsoft.EntityFrameworkCore;

namespace Phonebook.Infrastructure.Context
{
    public class ApplicationDbContext : IApplicationDbContext
    {
        private DbFactoryContext _dbFactoryContext;

        public ApplicationDbContext(DbFactoryContext dbFactoryContext)
        {
            _dbFactoryContext = dbFactoryContext;
        }

        public DbContext DbContext => _dbFactoryContext.DbContext;
    }
}
