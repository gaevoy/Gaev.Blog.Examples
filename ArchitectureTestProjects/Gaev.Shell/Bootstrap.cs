using Microsoft.Extensions.DependencyInjection;

namespace Gaev.Shell;

public class Bootstrap
{
    public static void Boot(IServiceCollection container)
    {
        Alfa.Bootstrap.Boot(container);
        Bravo.Bootstrap.Boot(container);
        Charlie.Bootstrap.Boot(container);
    }
}
