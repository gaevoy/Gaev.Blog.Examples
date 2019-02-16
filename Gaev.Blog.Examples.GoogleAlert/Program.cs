using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Telegram;

namespace Gaev.Blog.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build()
                .Get<Config>();
            using (var log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Telegram(config.TelegramApiKey, config.TelegramChatId)
                .CreateLogger())
            using (var db = new Db())
            {
                await db.Database.EnsureCreatedAsync();

                var newPages = await Search(config, "\"gaevoy.github.io\"");
                var oldPages = await db.Pages.ToListAsync();
                var added = newPages.Except(oldPages).ToList();
                var removed = oldPages.Except(newPages).ToList();

                foreach (var item in added)
                    log.Information($"Added [{item.Title}]({item.Link})");
                foreach (var item in removed)
                    log.Information($"Removed [{item.Title}]({item.Link})");

                db.Pages.AddRange(added);
                db.Pages.RemoveRange(removed);
                await db.SaveChangesAsync();
            }
        }

        static async Task<List<Page>> Search(Config config, string query)
        {
            var googleApi = new CustomsearchService(new BaseClientService.Initializer {ApiKey = config.GoogleApiKey});
            var request = googleApi.Cse.List(query);
            request.Cx = config.GoogleSearchEngineId;
            var response = await request.ExecuteAsync();
            return response.Items.Select(e => new Page {Title = e.Title, Link = e.Link}).ToList();
        }
    }

    class Config
    {
        // https://stackoverflow.com/a/44345657/1400547
        public string GoogleApiKey { get; set; }
        public string GoogleSearchEngineId { get; set; }

        // https://github.com/oxozle/serilog-sinks-telegram/issues/1
        public string TelegramApiKey { get; set; }
        public string TelegramChatId { get; set; }
    }

    class Db : DbContext
    {
        public DbSet<Page> Pages { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder opt) => opt.UseSqlite("Data Source=Alerts.db");
    }

    class Page
    {
        [Key] public string Link { get; set; }
        public string Title { get; set; }
        public override bool Equals(object compared) => string.Equals(Link, ((Page) compared).Link);
        public override int GetHashCode() => Link.GetHashCode();
    }
}