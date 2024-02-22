using Phonebook.Infrastructure.Extensions;
using System;
using System.Linq.Expressions;

namespace Phonebook.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    /// <summary>
    /// Where method of IRepository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IQueryable<T> Where<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate) where T : class
    => repository.Entities.Where(predicate);

    /// <summary>
    /// Get list by T entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <returns></returns>
    public static async Task<List<T>> ToListAsync<T>(this IRepository<T> repository) where T : class
    => await repository.ToListAsync();
    public static async Task<List<T>> ToListAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate) where T : class
    => await repository.Where(predicate).ToListAsync();
    public static IOrderedQueryable<T> OrderBy<T, TKey>(this IRepository<T> repository, Expression<Func<T, TKey>> keySelector) where T : class
    => repository.Entities.OrderBy(keySelector);
    public static async Task<T> FirstOrDefaultAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate) where T : class
    => await repository.Entities.FirstOrDefaultAsync(predicate);
    public static async Task<T> LastOrDefaultAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate) where T : class
    => await repository.Entities.LastOrDefaultAsync(predicate);
    /// <summary>
    /// Check existence of T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="predicate"></param>
    /// <returns>Return true if exists otherwise false</returns>
    public static async Task<bool> AnyAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate) where T : class
        => await repository.Entities.AnyAsync(predicate);
    public static async Task<int> CountAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate) where T : class
        => await repository.Entities.CountAsync(predicate);
    public static IQueryable<TResult> Select<T, TResult>(this IRepository<T> repository, Expression<Func<T, TResult>> selector) where T : class
    => repository.Entities.Select(selector);
}
