using Phonebook.Caching;
using System.Runtime.CompilerServices;
using Phonebook.Caching.Common;
using Phonebook.Caching.Extensions;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace Phonebook.Services.Services;

public class ContactService : DataServiceBase<Contact>, IContactService
{
    private readonly IDataCached dataCached;
    public ContactService(IUnitOfWork unitOfWork, IDataCached dataCached) : base(unitOfWork)
    {
        this.dataCached = dataCached;
    }

    public override async Task AddAsync(Contact entity)
    {
        string key = null!;
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            // luu vao database
            var id = await UnitOfWork.Repository<Contact>().InsertAsync(entity, e => e.ContactId);
            // luu vao cache
            key = string.Format(CachingCommonDefaults.CacheKey, typeof(Contact).Name, id);
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
            var entity = await UnitOfWork.Repository<Contact>().FindAsync(id) ?? throw new KeyNotFoundException();
            string key = string.Format(CachingCommonDefaults.CacheKey, typeof(Contact).Name, id);
            dataCached.Remove(key);
            await UnitOfWork.Repository<Contact>().DeleteAsync(entity);
            await UnitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public override async Task DeleteAsync(Contact entity)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            await UnitOfWork.Repository<Contact>().DeleteAsync(entity);
            await UnitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public override async Task<IList<Contact>> GetAllAsync()
    {
        string name = typeof(Contact).Name.ToLower();
        string keyAll = string.Format(CachingCommonDefaults.AllCacheKey, name);
        //1. check if data exists, if not load all data to cache
        if (!dataCached.IsSet(keyAll) || !(bool)dataCached.Get(keyAll))
        {
            await _loadAllContactToCached();
            dataCached.Set(keyAll, true, CachingCommonDefaults.CacheTime);
        }
        //2. get all data
        string pattern = string.Format(CachingCommonDefaults.CacheKeyHeader, name);
        var result = dataCached.GetValues<Contact>(pattern);
        return await Task.FromResult(result);
    }

    private async Task _loadAllContactToCached()
    {
        var Contacts = await UnitOfWork.Repository<Contact>().Entities.ToListAsync();
        string key = string.Empty;
        foreach (var p in Contacts)
        {
            key = dataCached.GetKey(p, p => p.ContactId).ToLower();
            if (!dataCached.IsSet(key))
                dataCached.Set(key, p, CachingCommonDefaults.CacheTime);
        }
    }
    public override async Task<IEnumerable<Contact>> GetAllAsync(Expression<Func<Contact, bool>> predicate) => await UnitOfWork.Repository<Contact>().Entities.Where(predicate).ToListAsync();

    public override async Task<Contact> GetOneAsync(int? id)
    {
        try
        {
            string key = string.Format(CachingCommonDefaults.CacheKey, typeof(Contact).Name.ToLower(), id);
            if (dataCached.IsSet(key))
                return await Task.FromResult(dataCached.GetT<Contact>(key));
            Contact? p = await UnitOfWork.Repository<Contact>().FindAsync(id!);
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

    public override async Task UpdateAsync(Contact entity)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync();
            var Contact = await UnitOfWork.Repository<Contact>().FindAsync(entity.ContactId);
            if (Contact == null) throw new KeyNotFoundException();
            var entityType = entity.GetType();
            var ContactType = Contact.GetType();

            foreach (var property in entityType.GetProperties())
            {
                var entityValue = property.GetValue(entity);
                var ContactProperty = ContactType.GetProperty(property.Name);

                // Check if the property exists in the Contact object and if the values are different
                if (ContactProperty != null && !Equals(entityValue, ContactProperty.GetValue(Contact)))
                {
                    ContactProperty.SetValue(Contact, entityValue);
                }
            }
            await UnitOfWork.Repository<Contact>().UpdateAsync(Contact);
            string key = string.Format(CachingCommonDefaults.CacheKey, typeof(Contact).Name, entity.ContactId);
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
