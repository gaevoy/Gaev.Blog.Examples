using Gaev.Alfa.Api;
using Gaev.Bravo.Api;

namespace Gaev.Bravo;

public class Domain : IBravoApi
{
    private readonly IAlfaApi _alfaApi;

    public Domain(IAlfaApi alfaApi)
    {
        _alfaApi = alfaApi;
    }
}
