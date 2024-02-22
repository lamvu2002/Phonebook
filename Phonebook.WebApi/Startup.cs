using Microsoft.EntityFrameworkCore;
using Phonebook.Caching.Extensions;
using Phonebook.Services.Extensions;
using Phonebook.WebApi.Security;
using Microsoft.EntityFrameworkCore;
using Phonebook.Shared;
using Phonebook.IdentityJWT.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace Phonebook.WebApi;

public class Startup
{

    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddMemoryCache();

        // call services mannual
        services.PhonebookInfrastructureDatabase(Configuration);
        services.AddDataServices();
        services.AddCacheServices();

        // Security
        services.AddCors();

        // Intergrate authenticate
        //ket noi xuong db
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("PhonebookDbConn")));
        // for identity
        services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
        // add authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = Configuration["JWT:ValidAudience"],
                ValidIssuer = Configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
            };
        });
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Phonebook v1"));
        }
        else
        {
            app.UseHsts();
        }
        app.UseMiddleware<SecurityHeader>();
        app.UseHttpsRedirection();
        app.UseRouting();
        //authentication
        app.UseAuthentication();

        app.UseAuthorization();
        app.UseCors(configurePolicy: option =>
        {
            option.WithMethods("GET", "POST", "PUT", "DELETE");
            option.WithOrigins("http://localhost:5037");
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        //app.MapControllers();
    }
}
