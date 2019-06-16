using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Gaev.Blog.Examples.Controllers
{
    [Route("session")]
    public class SessionController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<SignatureResult>> Clients =
            new ConcurrentDictionary<string, TaskCompletionSource<SignatureResult>>();

        [HttpPost("{challenge}")]
        public async Task SignIn(string challenge)
        {
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";
            var onResultReady = Clients[challenge] = new TaskCompletionSource<SignatureResult>();
            HttpContext.RequestAborted.Register(() => onResultReady.SetResult(new SignatureResult()));
            var result = await onResultReady.Task;
            Clients.TryRemove(challenge, out _);
            if (result.IsSignatureValid)
            {
                var identity = new GenericIdentity(result.KeyOwner, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
            }
        }

        [HttpPut("{challenge}")]
        public async Task ProveKeyOwnership(string challenge)
        {
            string body = new StreamReader(Request.Body).ReadToEnd();
            PgpSignature signature = ParsePgpSignature(body);
            KeybaseKey keybaseKey = await GetPublicKeyFromKeybase(signature.KeyId);
            PgpPublicKey publicKey = ParsePgpPublicKey(keybaseKey.Bundle, signature.KeyId);
            bool isSignatureValid = VerifySignature(publicKey, challenge, signature);
            if (!Clients.TryGetValue(challenge, out var client))
                return;
            client.SetResult(new SignatureResult
            {
                KeyOwner = keybaseKey.Username,
                IsSignatureValid = isSignatureValid
            });
        }

        [HttpGet]
        public dynamic GetCurrentUser()
        {
            return new
            {
                User.Identity.IsAuthenticated,
                User.Identity.Name
            };
        }

        [HttpDelete]
        public async Task SignOut()
        {
            await HttpContext.SignOutAsync();
        }

        private static bool VerifySignature(PgpPublicKey publicKey, string payload, PgpSignature signature)
        {
            signature.InitVerify(publicKey);
            signature.Update(Encoding.UTF8.GetBytes(payload));
            return signature.Verify();
        }

        private static PgpSignature ParsePgpSignature(string pgpSignature)
        {
            var input = PgpUtilities.GetDecoderStream(new MemoryStream(Encoding.UTF8.GetBytes(pgpSignature)));
            var objectFactory = new PgpObjectFactory(input);
            var signatureList = (PgpSignatureList) objectFactory.NextPgpObject();
            return signatureList[0];
        }

        private static PgpPublicKey ParsePgpPublicKey(string publicKey, long keyId)
        {
            var input = PgpUtilities.GetDecoderStream(new MemoryStream(Encoding.UTF8.GetBytes(publicKey)));
            var pgpRings = new PgpPublicKeyRingBundle(input);
            return pgpRings.GetPublicKey(keyId);
        }

        private async Task<KeybaseKey> GetPublicKeyFromKeybase(long keyId)
        {
            using (var cli = new HttpClient())
            {
                var url = $"https://keybase.io/_/api/1.0/key/fetch.json?pgp_key_ids={keyId:x8}";
                var json = await cli.GetStringAsync(url);
                var data = JsonConvert.DeserializeObject<KeybaseKeys>(json);
                return data.Keys.FirstOrDefault();
            }
        }

        public class KeybaseKeys
        {
            public KeybaseKey[] Keys { get; set; }
        }

        public class KeybaseKey
        {
            public string Bundle { get; set; }
            public string Username { get; set; }
        }

        public class SignatureResult
        {
            public string KeyOwner { get; set; }
            public bool IsSignatureValid { get; set; }
        }
    }
}