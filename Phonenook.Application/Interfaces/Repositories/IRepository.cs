namespace Phonebook.Application.Interfaces.Repositories;

public interface IRepository<T> : IBaseReaderRepository<T>,
    IBaseWriteRepository<T>,
    IBaseRepo<T> where T : class
{

}
