using Gaev.Alfa.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Gaev.Alfa;

public class Bootstrap
{
    public static void Boot(IServiceCollection container)
    {
        container.AddScoped<IApi, Domain>();
    }
}
