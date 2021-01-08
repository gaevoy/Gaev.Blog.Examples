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
            Console.WriteLine($@"
AppName:               {config["AppName"]}
DbConnectionString:    {config["DbConnectionString"]}
CertificatePassphrase: {config["CertificatePassphrase"]}
SendGrid.ApiKey:       {config["SendGrid:ApiKey"]}
Partner.SftpUrl:       {config["Partner:SftpUrl"]}
");
        }
    }
}