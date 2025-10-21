using Application_lib;
using Application_lib.Authorization;
using Application_lib.Gitlab;
using Infrastructure_lib.Gitlab;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure_lib
{
    public static class ServicesDI
    {
        public static IServiceCollection ServicesDIExtention(this IServiceCollection services)
        {
            services.AddScoped<IGitService, GitService>();
            services.AddScoped<IAuthService, AuthorizationService>();
            services.AddScoped<ITextFormatterService, TextFormatterService>();
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }
    }
}
