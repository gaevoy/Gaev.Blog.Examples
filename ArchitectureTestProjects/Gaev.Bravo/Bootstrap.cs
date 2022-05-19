using Gaev.Bravo.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Gaev.Bravo;

public class Bootstrap
{
    public static void Boot(IServiceCollection container)
    {
        container.AddScoped<IBravoApi, Domain>();
    }
}
