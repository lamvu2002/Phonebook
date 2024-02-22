namespace Phonebook.Application.Base;

public interface IBaseRepo<T> where T : class
{
    DbSet<T> Entities { get; }
    IApplicationDbContext ApplicationDbContext { get; }
}
