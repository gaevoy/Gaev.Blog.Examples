using System;
using Microsoft.Extensions.Configuration;

namespace Gaev.Blog.SecuredAppSettingsJson
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build()
                .Decrypt(keyPath: "CipherKey", cipherPrefix: "CipherText:");
            Console.WriteLine(config["ConnectionString"]);
        }
    }
}