using Gaev.Blog.Examples.PiiManagement.PiiSerializers;
using NUnit.Framework;
using Serilog;

namespace Gaev.Blog.Examples.PiiManagement.Serilog;

public class PiiStringTests
{
    [Test]
    public void Serilog_should_work()
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .Destructure.ByTransforming<PiiString>(e => PiiScope.Serializer.ToString(e))
            .CreateLogger();

        using var _ = new PiiScope(new Sha256());
        var user = new User
        {
            Name = "John Doe",
            Email = "john.doe@test.com"
        };
        logger.Information("The user is {@Data}", user);
        logger.Information("The email is {@Data}", user.Email);
        logger.Information("The email is {@Data}", new { user.Email });
    }
}