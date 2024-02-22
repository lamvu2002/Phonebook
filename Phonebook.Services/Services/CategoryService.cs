using Phonebook.Caching;
using System.Runtime.CompilerServices;
using Phonebook.Caching.Common;
using Phonebook.Caching.Extensions;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace Phonebook.Services.Services;

public class CategoryService : DataServiceBase<Category>, ICategoryService
{
    private readonly IDataCached dataCached;
    public CategoryService(IUnitOfWork unitOfWork, IDataCached dataCached) : base(unitOfWork)
    {
        this.dataCached = dataCached;
    }

    public override async Task AddAsync(Category entity)
    {
        string key = null!;
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            // luu vao database
            var id = await UnitOfWork.Repository<Category>().InsertAsync(entity, e => e.CategoryId);
            // luu vao cache
            key = string.Format(CachingCommonDefaults.CacheKey, typeof(Category).Name, id);
            dataCached.Set(key, entity, CachingCommonDefaults.CacheTime);
            await UnitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            dataCached.Remove(key);
            throw;
        }
    }


    public override async Task DeleteAsync(int? id)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            var entity = await UnitOfWork.Repository<Category>().FindAsync(id) ?? throw new KeyNotFoundException();
            string key = string.Format(CachingCommonDefaults.CacheKey, typeof(Category).Name, id);
            dataCached.Remove(key);
            await UnitOfWork.Repository<Category>().DeleteAsync(entity);
            await UnitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public override async Task DeleteAsync(Category entity)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            await UnitOfWork.Repository<Category>().DeleteAsync(entity);
            await UnitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public override async Task<IList<Category>> GetAllAsync()
    {
        string name = typeof(Category).Name.ToLower();
        string keyAll = string.Format(CachingCommonDefaults.AllCacheKey, name);
        //1. check if data exists
        if (!dataCached.IsSet(keyAll) || !(bool)dataCached.Get(keyAll))
        {
            await _loadAllCategoryToCached();
            dataCached.Set(keyAll, true, CachingCommonDefaults.CacheTime);
        }
        //2. get all data
        string pattern = string.Format(CachingCommonDefaults.CacheKeyHeader, name);
        var result = dataCached.GetValues<Category>(pattern);
        return await Task.FromResult(result);
    }

    private async Task _loadAllCategoryToCached()
    {
        var Categorys = await UnitOfWork.Repository<Category>().Entities.ToListAsync();
        string key = string.Empty;
        foreach (var p in Categorys)
        {
            key = dataCached.GetKey(p, p => p.CategoryId).ToLower();
            if (!dataCached.IsSet(key))
                dataCached.Set(key, p, CachingCommonDefaults.CacheTime);
        }
    }
    public override async Task<IEnumerable<Category>> GetAllAsync(Expression<Func<Category, bool>> predicate) => await UnitOfWork.Repository<Category>().Entities.Where(predicate).ToListAsync();

    public override async Task<Category> GetOneAsync(int? id)
    {
        try
        {
            string key = string.Format(CachingCommonDefaults.CacheKey, typeof(Category).Name.ToLower(), id);
            if (dataCached.IsSet(key))
                return await Task.FromResult(dataCached.GetT<Category>(key));
            Category? p = await UnitOfWork.Repository<Category>().FindAsync(id!);
            if (p != null)
            {
                dataCached.Set(key, p, CachingCommonDefaults.CacheTime);
                return await Task.FromResult(p);
            }
            return null!;
        }
        catch (Exception ex)
        {
            return null!;
        }
    }

    public override async Task UpdateAsync(Category entity)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            var Category = await UnitOfWork.Repository<Category>().FindAsync(entity.CategoryId);
            if (Category == null) throw new KeyNotFoundException();
            var entityType = entity.GetType();
            var CategoryType = Category.GetType();

            foreach (var property in entityType.GetProperties())
            {
                var entityValue = property.GetValue(entity);
                var CategoryProperty = CategoryType.GetProperty(property.Name);

                // Check if the property exists in the Category object and if the values are different
                if (CategoryProperty != null && !Equals(entityValue, CategoryProperty.GetValue(Category)))
                {
                    CategoryProperty.SetValue(Category, entityValue);
                }
            }
            await UnitOfWork.Repository<Category>().UpdateAsync(Category);
            string key = string.Format(CachingCommonDefaults.CacheKey, typeof(Category).Name, entity.CategoryId);
            dataCached.Set(key, entity, CachingCommonDefaults.CacheTime);
            await UnitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
