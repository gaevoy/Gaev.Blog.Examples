using Gaev.Charlie.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Gaev.Charlie;

public class Bootstrap
{
    public static void Boot(IServiceCollection container)
    {
        container.AddScoped<ICharlieApi, Domain>();
    }
}
