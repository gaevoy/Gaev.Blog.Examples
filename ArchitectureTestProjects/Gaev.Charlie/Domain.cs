using Gaev.Alfa.Api;
using Gaev.Bravo.Api;
using Gaev.Charlie.Api;

namespace Gaev.Charlie;

public class Domain : ICharlieApi
{
    private readonly IAlfaApi _alfaApi;
    private readonly IBravoApi _bravoApi;

    public Domain(IAlfaApi alfaApi, IBravoApi bravoApi)
    {
        _alfaApi = alfaApi;
        _bravoApi = bravoApi;
    }
}
