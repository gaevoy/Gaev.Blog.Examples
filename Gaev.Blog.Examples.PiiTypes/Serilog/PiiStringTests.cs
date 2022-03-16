using NUnit.Framework;
using Serilog;

namespace Gaev.Blog.Examples.Serilog;

public class PiiStringTests
{
    [Test]
    public void Serilog_should_work()
    {
        var sha256 = new PiiAsSha256();
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .Destructure.ByTransforming<PiiString>(e => sha256.ToSystemString(e))
            .CreateLogger();

        var user = new User
        {
            Name = "John Doe",
            Email = "john.doe@test.com"
        };
        logger.Information("The user is {@Data}", user);
        logger.Information("The email is {@Data}", user.Email);
        logger.Information("The email is {@Data}", new {user.Email});
    }
}