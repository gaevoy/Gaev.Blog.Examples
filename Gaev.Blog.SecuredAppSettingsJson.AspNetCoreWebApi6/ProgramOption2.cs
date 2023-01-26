using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Gaev.Blog.SecuredAppSettingsJson.AspNetCoreWebApi6;

public static class Program
{
    public static void Main(string[] args)
    {
        WebHost
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(appConfig =>
            {
                appConfig.AddEnvironmentVariables(prefix: "APPPREFIX_");
            })
            .UseStartup<Startup>()
            .Build()
            .Run();
    }

    public class Startup
    {
        public Startup(IConfiguration cfg)
        {
            // This will decrypt the configuration
            ((IConfigurationRoot) cfg).Decrypt(keyPath: "CipherKey", cipherPrefix: "CipherText:");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(c => c.MapControllers());
        }
    }
}

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly IConfiguration _config;

    public TestController(IConfiguration config)
        => _config = config;

    [HttpGet]
    public string TestConfig()
        => _config["DbConnectionString"];
}
