using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Gaev.Blog.Examples.Controllers
{
    [Route("api")]
    public class ValuesController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, List<StreamWriter>> Members =
            new ConcurrentDictionary<string, List<StreamWriter>>();

        [HttpGet("{challenge}")]
        public async Task ListenForSignedChallenge(string challenge)
        {
            Response.Headers["Cache-Control"] = "no-cache"; // https://serverfault.com/a/801629
            Response.Headers["X-Accel-Buffering"] = "no";
            Response.ContentType = "text/event-stream";
            using (var member = new StreamWriter(Response.Body))
            {
                var members = Members.GetOrAdd(challenge, _ => new List<StreamWriter>(4));
                lock (members)
                    members.Add(member);
                try
                {
                    await Task.Delay(Timeout.Infinite, HttpContext.RequestAborted);
                }
                catch (TaskCanceledException)
                {
                }

                lock (members)
                    members.Remove(member);
            }
        }

        [HttpPost("{challenge}")]
        public async Task SendMessage(string challenge)
        {
            var message = new StreamReader(Request.Body).ReadToEnd();
            if (!Members.TryGetValue(challenge, out var members))
                return;

            lock (members)
                members = members.ToList(); // copy to be thread safe

            async Task Send(StreamWriter member)
            {
                try
                {
                    await member.WriteAsync("data: " + JsonConvert.SerializeObject(message) + "\n\n");
                    await member.FlushAsync();
                }
                catch (ObjectDisposedException)
                {
                    lock (members)
                        members.Remove(member);
                }
            }

            await Task.WhenAll(members.Select(Send));
        }
    }
}