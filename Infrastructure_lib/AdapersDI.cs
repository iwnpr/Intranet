using Application_lib.Authorization;
using Application_lib.Gitlab;
using Infrastructure_lib.Gitlab;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure_lib
{
    public static class AdapersDI
    {
        public static IServiceCollection AdaptersDIExtention(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDBContext>(x => x.UseNpgsql(configuration.GetConnectionString("TaskerDB")));
            services.AddScoped<IGitDBService, GitDBSync>();
            services.AddScoped<IGitApiService, GitApiData>();
            services.AddScoped<ILdapAuthService, LdapAuthService>();
            return services;
        }
    }
}
