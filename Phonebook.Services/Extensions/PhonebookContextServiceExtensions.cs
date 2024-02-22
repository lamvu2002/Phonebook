using Microsoft.Extensions.Options;
using Phonebook.Services.Services;

namespace Phonebook.Services.Extensions;

public static class PhonebookContextServiceExtensions
{
    //1. Tao ra 1 dich vu de thuc hien ket noi database
    public static IServiceCollection PhonebookInfrastructureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PhonebookContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("PhonebookDbConn"), sqlOptions =>
            {
                sqlOptions.CommandTimeout(60);
            });
            // options.UseLazyLoadingProxies();
        });
        //2. add dbcontext by service
        services.AddScoped<Func<PhonebookContext>>(provider => () => provider.GetService<PhonebookContext>());
        services.AddScoped<DbFactoryContext>();
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
    // 2. data services
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISubcategoryService, SubcategoryService>();
        return services;
    }
}
