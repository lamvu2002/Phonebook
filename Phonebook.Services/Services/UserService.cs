using Phonebook.Caching;
using System.Runtime.CompilerServices;
using Phonebook.Caching.Common;
using Phonebook.Caching.Extensions;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace Phonebook.Services.Services;

public class UserService : DataServiceBase<AspNetUser>, IUserService
{
    private readonly IDataCached dataCached;
    public UserService(IUnitOfWork unitOfWork, IDataCached dataCached) : base(unitOfWork)
    {
        this.dataCached = dataCached;
    }

    public override async Task<IList<AspNetUser>> GetAllAsync()
    {
        string name = typeof(AspNetUser).Name.ToLower();
        string keyAll = string.Format(CachingCommonDefaults.AllCacheKey, name);
        //1. check if data exists
        if (!dataCached.IsSet(keyAll) || !(bool)dataCached.Get(keyAll))
        {
            await _loadAllUserToCached();
            dataCached.Set(keyAll, true, CachingCommonDefaults.CacheTime);
        }
        //2. get all data
        string pattern = string.Format(CachingCommonDefaults.CacheKeyHeader, name);
        var result = dataCached.GetValues<AspNetUser>(pattern);
        return await Task.FromResult(result);
    }

    private async Task _loadAllUserToCached()
    {
        var Users = await UnitOfWork.Repository<AspNetUser>().Entities.ToListAsync();
        string key = string.Empty;
        foreach (var p in Users)
        {
            key = dataCached.GetKey(p, p => p.Id).ToLower();
            if (!dataCached.IsSet(key))
                dataCached.Set(key, p, CachingCommonDefaults.CacheTime);
        }
    }
    public override async Task<IEnumerable<AspNetUser>> GetAllAsync(Expression<Func<AspNetUser, bool>> predicate) => await UnitOfWork.Repository<AspNetUser>().Entities.Where(predicate).ToListAsync();

    public override Task<AspNetUser> GetOneAsync(int? id)
    {
        throw new NotImplementedException();
    }

    public override Task UpdateAsync(AspNetUser entity)
    {
        throw new NotImplementedException();
    }

    public override Task AddAsync(AspNetUser entity)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteAsync(int? id)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteAsync(AspNetUser entity)
    {
        throw new NotImplementedException();
    }
}

    
