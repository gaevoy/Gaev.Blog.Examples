using System.Text;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;

namespace Gaev.Blog.Examples.NLog;

public class PiiStringTests
{
    [Test]
    public void NLog_should_work()
    {
        LogManager.Setup().SetupSerialization(s =>
        {
            var jsonConverter = new NewtonsoftJson.PiiStringConverter(new PiiAsSha256());
            var settings = new JsonSerializerSettings {Converters = {jsonConverter}};
            s.RegisterJsonConverter(new NewtonsoftJsonConverter(settings));
        });

        var logger = new LogFactory(WriteToConsoleConfig()).GetCurrentClassLogger();
        var user = new User
        {
            Name = "John Doe",
            Email = "john.doe@test.com"
        };
        logger.Info("The user is {@Data}", user);
        logger.Info("The email is {@Data}", user.Email);
        logger.Info("The email is {@Data}", new {user.Email});
    }

    [Test]
    public void NLog_should_work_for_JsonLayout()
    {
        LogManager.Setup().SetupSerialization(s =>
        {
            var jsonConverter = new NewtonsoftJson.PiiStringConverter(new PiiAsSha256());
            var settings = new JsonSerializerSettings {Converters = {jsonConverter}};
            s.RegisterJsonConverter(new NewtonsoftJsonConverter(settings));
        });

        var logger = new LogFactory(WriteJsonToConsoleConfig()).GetCurrentClassLogger();
        var user = new User
        {
            Name = "John Doe",
            Email = "john.doe@test.com"
        };
        logger.Info("The user is {@Data}", user);
        logger.Info("The email is {@Data}", user.Email);
        logger.Info("The email is {@Data}", new {user.Email});
    }

    private static LoggingConfiguration WriteToConsoleConfig()
    {
        var config = new LoggingConfiguration();
        var target = new ColoredConsoleTarget("console")
        {
            Layout = @"[${date:format=HH\:mm\:ss} ${level}] ${message}${newline}${exception:format=ToString}"
        };
        config.AddTarget(target);
        config.AddRuleForAllLevels(target);
        return config;
    }

    private static LoggingConfiguration WriteJsonToConsoleConfig()
    {
        var config = new LoggingConfiguration();
        var target = new ColoredConsoleTarget("console")
        {
            Layout = new JsonLayout
            {
                IncludeAllProperties = true,
                Attributes =
                {
                    new JsonAttribute("Message", "${message:raw=true}"),
                    new JsonAttribute("Date", "${date:universalTime=True:format=O}"),
                    new JsonAttribute("Logger", "${logger}"),
                    new JsonAttribute("Level", "${level:upperCase=true}"),
                    new JsonAttribute("Exception", "${exception:format=toString}"),
                }
            }
        };
        config.AddTarget(target);
        config.AddRuleForAllLevels(target);
        return config;
    }
}

public class NewtonsoftJsonConverter : IJsonConverter
{
    private readonly JsonSerializerSettings _settings;

    public NewtonsoftJsonConverter(JsonSerializerSettings settings)
    {
        _settings = settings;
    }

    public bool SerializeObject(object value, StringBuilder builder)
    {
        builder.Append(JsonConvert.SerializeObject(value, _settings));
        return true;
    }
}