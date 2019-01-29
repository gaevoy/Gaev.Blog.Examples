using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using NUnit.Framework;
using Owin;
using SendGrid;
using SendGrid.Helpers.Mail;
#pragma warning disable 1998
#pragma warning disable 4014

namespace Gaev.Blog.Examples
{
    public class SendGridClientTests
    {
        private const string TestRecipientEmail = "...";
        private const string ApiKey = "...";

        [Test]
        public async Task It_should_listen_for_webhooks()
        {
            // Given
            var messageId = Guid.NewGuid().ToString();
            var onWebhookReceived = new TaskCompletionSource<Webhook>();
            Task.Delay(TimeSpan.FromMinutes(3))
                .ContinueWith(_ => onWebhookReceived.TrySetCanceled());

            async Task OnWebhookAppeared(IOwinContext ctx, Func<Task> _)
            {
                var json = new StreamReader(ctx.Request.Body).ReadToEnd();
                var webhook = JsonConvert.DeserializeObject<Webhook[]>(json)
                    .FirstOrDefault(e => e.MyId == messageId);
                if (webhook != null)
                    onWebhookReceived.TrySetResult(webhook);
            }

            using (var localApi = new HttpServer().Use(OnWebhookAppeared).Start())
            using (var ngrok = new NgrokTunnel(localPort: localApi.ApiBaseUrl.Port))
            {
                // When
                await RegisterWebhookUrl(ngrok.PublicTunnelUrl);
                await SendTestEmail(messageId);

                // Then
                var webhook = await onWebhookReceived.Task;
                Assert.That(webhook, Is.Not.Null);
                Assert.That(webhook.Event, Is.EqualTo("processed"));
            }
        }

        private async Task SendTestEmail(string messageId)
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress("test@example.com"),
                Subject = "Sending with SendGrid is Fun",
                PlainTextContent = "and easy to do anywhere, even with C#",
                CustomArgs = new Dictionary<string, string> {{"MyId", messageId}}
            };
            msg.AddTo(new EmailAddress(TestRecipientEmail));
            await new SendGridClient(ApiKey).SendEmailAsync(msg);
        }

        private static async Task RegisterWebhookUrl(string publicUrl)
        {
            var url = "https://api.sendgrid.com/v3/user/webhooks/event/settings";
            var data = new
            {
                enabled = true,
                url = publicUrl,
                delivered = true,
                processed = true,
                open = true
            };
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };
            await new SendGridClient(ApiKey).MakeRequest(request);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        public class Webhook
        {
            public string Event { get; set; }
            public string MyId { get; set; }
        }
    }

    public class HttpServer : IDisposable
    {
        private static readonly Random Random =
            new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray().Take(4).ToArray(), 0));

        private IDisposable _server;
        private List<Action<IAppBuilder>> _builders = new List<Action<IAppBuilder>>();

        public HttpServer Configure(Action<IAppBuilder> builder)
        {
            _builders.Add(builder);
            return this;
        }

        public HttpServer Use(Func<IOwinContext, Func<Task>, Task> handler)
        {
            return Configure(appBuilder => appBuilder.Use<UseHandlerMiddleware>(handler));
        }

        public HttpServer Start(Uri apiBaseUrl = null)
        {
            var count = 0;
            CallContext.LogicalSetData(nameof(HttpServer), _builders);
            _builders = new List<Action<IAppBuilder>>();
            while (true)
            {
                try
                {
                    ApiBaseUrl = apiBaseUrl ?? new Uri($"http://localhost:{Random.Next(1000, 9999)}/");
                    var url = ApiBaseUrl.ToString().Replace("localhost", "*");
                    _server = WebApp.Start<HttpServer>(url);
                    break;
                }
                catch
                {
                    if (count++ == 2) throw;
                }
            }

            return this;
        }

        public Uri ApiBaseUrl { get; private set; }

        public static void Configuration(IAppBuilder appBuilder)
        {
            var builders = CallContext.LogicalGetData(nameof(HttpServer)) as List<Action<IAppBuilder>>;
            if (builders != null)
            {
                foreach (var builder in builders)
                    builder(appBuilder);
                CallContext.FreeNamedDataSlot(nameof(HttpServer));
            }
        }

        public void Dispose()
        {
            _server?.Dispose();
            _server = null;
        }
    }

    public class NgrokTunnel : IDisposable
    {
        private readonly Process _process;

        public NgrokTunnel(int localPort)
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo("ngrok", "http " + localPort)
                {
                    WindowStyle = ProcessWindowStyle.Minimized
                }
            };
            _process.Start();
        }

        public string PublicTunnelUrl
        {
            get
            {
                for (int i = 0; i <= 10; i++)
                    try
                    {
                        var ngrokApi = "http://127.0.0.1:4040/api/tunnels/command_line";
                        var json = new WebClient().DownloadString(ngrokApi);
                        return (string) JsonConvert.DeserializeObject<dynamic>(json).public_url;
                    }
                    catch (Exception)
                    {
                        if (i == 10) throw;
                        Thread.Sleep(200);
                    }

                throw new TimeoutException();
            }
        }

        public void Dispose()
        {
            _process.Kill();
            _process.Dispose();
        }
    }
}