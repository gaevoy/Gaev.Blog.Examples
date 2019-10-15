using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Gaev.Blog.Examples.FileReceiver
{
    public class Program
    {
        public static void Main(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(opt => { opt.Limits.MaxRequestBodySize = null; })
                .Build()
                .Run();
    }

    public class Startup
    {
        // tar zcf - /var/www | curl --data-binary @- http://f81512d0.ngrok.io/www.tar.gz
        // zip -rq - /var/www | curl --data-binary @- http://f81512d0.ngrok.io/www.zip
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Run(async ctx =>
            {
                using (var file = File.OpenWrite(Path.GetFileName(ctx.Request.Path)))
                {
                    await ctx.Request.Body.CopyToAsync(file);
                    await ctx.Response.WriteAsync($"Received in `{file.Name}`\n");
                }
            });
        }
    }
}