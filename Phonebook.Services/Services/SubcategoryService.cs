using Phonebook.Caching;
using System.Runtime.CompilerServices;
using Phonebook.Caching.Common;
using Phonebook.Caching.Extensions;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace Phonebook.Services.Services;

public class SubcategoryService : DataServiceBase<Subcategory>, ISubcategoryService
{
    private readonly IDataCached dataCached;
    public SubcategoryService(IUnitOfWork unitOfWork, IDataCached dataCached) : base(unitOfWork)
    {
        this.dataCached = dataCached;
    }

    public override async Task AddAsync(Subcategory entity)
    {
        string key = null!;
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            // luu vao database
            var id = await UnitOfWork.Repository<Subcategory>().InsertAsync(entity, e => e.SubcategoryId);
            // luu vao cache
            key = string.Format(CachingCommonDefaults.CacheKey, typeof(Subcategory).Name, id);
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
            var entity = await UnitOfWork.Repository<Subcategory>().FindAsync(id) ?? throw new KeyNotFoundException();
            string key = string.Format(CachingCommonDefaults.CacheKey, typeof(Subcategory).Name, id);
            dataCached.Remove(key);
            await UnitOfWork.Repository<Subcategory>().DeleteAsync(entity);
            await UnitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public override async Task DeleteAsync(Subcategory entity)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            await UnitOfWork.Repository<Subcategory>().DeleteAsync(entity);
            await UnitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public override async Task<IList<Subcategory>> GetAllAsync()
    {
        string name = typeof(Subcategory).Name.ToLower();
        string keyAll = string.Format(CachingCommonDefaults.AllCacheKey, name);
        //1. check if data exists
        if (!dataCached.IsSet(keyAll) || !(bool)dataCached.Get(keyAll))
        {
            await _loadAllSubcategoryToCached();
            dataCached.Set(keyAll, true, CachingCommonDefaults.CacheTime);
        }
        //2. get all data
        string pattern = string.Format(CachingCommonDefaults.CacheKeyHeader, name);
        var result = dataCached.GetValues<Subcategory>(pattern);
        return await Task.FromResult(result);
    }

    private async Task _loadAllSubcategoryToCached()
    {
        var Subcategorys = await UnitOfWork.Repository<Subcategory>().Entities.ToListAsync();
        string key = string.Empty;
        foreach (var p in Subcategorys)
        {
            key = dataCached.GetKey(p, p => p.SubcategoryId).ToLower();
            if (!dataCached.IsSet(key))
                dataCached.Set(key, p, CachingCommonDefaults.CacheTime);
        }
    }
    public override async Task<IEnumerable<Subcategory>> GetAllAsync(Expression<Func<Subcategory, bool>> predicate) => await UnitOfWork.Repository<Subcategory>().Entities.Where(predicate).ToListAsync();

    public override async Task<Subcategory> GetOneAsync(int? id)
    {
        try
        {
            string key = string.Format(CachingCommonDefaults.CacheKey, typeof(Subcategory).Name.ToLower(), id);
            if (dataCached.IsSet(key))
                return await Task.FromResult(dataCached.GetT<Subcategory>(key));
            Subcategory? p = await UnitOfWork.Repository<Subcategory>().FindAsync(id!);
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

    public override async Task UpdateAsync(Subcategory entity)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            var Subcategory = await UnitOfWork.Repository<Subcategory>().FindAsync(entity.SubcategoryId);
            if (Subcategory == null) throw new KeyNotFoundException();
            var entityType = entity.GetType();
            var SubcategoryType = Subcategory.GetType();

            foreach (var property in entityType.GetProperties())
            {
                var entityValue = property.GetValue(entity);
                var SubcategoryProperty = SubcategoryType.GetProperty(property.Name);

                // Check if the property exists in the Subcategory object and if the values are different
                if (SubcategoryProperty != null && !Equals(entityValue, SubcategoryProperty.GetValue(Subcategory)))
                {
                    SubcategoryProperty.SetValue(Subcategory, entityValue);
                }
            }
            await UnitOfWork.Repository<Subcategory>().UpdateAsync(Subcategory);
            string key = string.Format(CachingCommonDefaults.CacheKey, typeof(Subcategory).Name, entity.SubcategoryId);
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
