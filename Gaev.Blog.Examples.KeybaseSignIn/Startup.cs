using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Gaev.Blog.Examples
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services
                .AddAuthentication(opt => { opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; })
                .AddCookie(opt => { });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) =>
            app
                .UseMvc()
                .UseAuthentication()
                .UseDefaultFiles()
                .UseStaticFiles();
    }
}