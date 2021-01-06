using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Gaev.Blog.SecuredAppSettingsJson
{
    public class UtilityTests
    {
        private static string CipherKey => "l8cpD27QcWDXjAg8ut+qH0IkWv/p38DrAst4Ee83jMg=";

        [TestCase("This is kind of connection string ;)")]
        public void Encrypt_value_and_print(string value)
        {
            var cipher = new Aes256Cipher(CipherKey);
            Console.WriteLine(cipher.Encrypt(value));
        }

        [TestCase("09lXf8qen+mQJeAgl7lBcTIdCvpvDOQs7NL3oyiwOJpfqn26PWxkpEkS2+SAGf0BjCHT/uHfXzYZPQeyYyb+0A==")]
        public void Decrypt_value_and_print(string value)
        {
            var cipher = new Aes256Cipher(CipherKey);
            Console.WriteLine(cipher.Decrypt(value));
        }

        [Test]
        public void Generate_new_key_and_print()
        {
            Console.WriteLine("Key: " + Aes256Cipher.GenerateNewKey());
        }

        [TestCase("../../../appsettings.json")]
        public void Encrypt_secured_fields_in_json(string filename)
        {
            var cipher = new Aes256Cipher(CipherKey);
            var jsonFile = new FileInfo(filename);
            var json = File.ReadAllText(jsonFile.FullName);
            json = Regex.Replace(
                json,
                @"""CipherText:(?<Text>[^""]+)""",
                m =>
                {
                    var text = m.Groups["Text"].Value;
                    try
                    {
                        var cipherText = cipher.Encrypt(text);
                        return $@"""CipherText:{cipherText}""";
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($@"Failed encrypting ""{text}""");
                        return m.Value;
                    }
                });
            File.WriteAllText(jsonFile.FullName, json);
        }

        [TestCase("../../../appsettings.json")]
        public void Decrypt_secured_fields_in_json(string filename)
        {
            var cipher = new Aes256Cipher(CipherKey);
            var jsonFile = new FileInfo(filename);
            var json = File.ReadAllText(jsonFile.FullName);
            json = Regex.Replace(
                json,
                @"""CipherText:(?<CipherText>[^""]+)""",
                m =>
                {
                    var cipherText = m.Groups["CipherText"].Value;
                    try
                    {
                        var text = cipher.Decrypt(cipherText);
                        return $@"""CipherText:{text}""";
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($@"Failed decrypting ""{cipherText}""");
                        return m.Value;
                    }
                });
            File.WriteAllText(jsonFile.FullName, json);
        }
    }
}