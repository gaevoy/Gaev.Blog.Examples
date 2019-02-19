using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
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
            using (var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Telegram(config.TelegramApiKey, config.TelegramChatId)
                .CreateLogger())
                try
                {
                    await Sync(logger);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "");
                }
        }

        static async Task Sync(Logger logger)
        {
            var otodom = new OtoDomCrawler();
            using (var db = new AlertsDatabase())
            {
                await db.Database.EnsureCreatedAsync();

                var foundPages1 = otodom.Search(
                    "https://www.otodom.pl/sprzedaz/mieszkanie/krakow/pradnik-czerwony/?search%5Bfilter_float_price%3Afrom%5D=300000&search%5Bfilter_float_price%3Ato%5D=600000&search%5Bfilter_enum_rooms_num%5D%5B0%5D=3&search%5Bfilter_enum_rooms_num%5D%5B1%5D=4&search%5Bfilter_enum_market%5D%5B0%5D=secondary&search%5Bdescription%5D=1&search%5Bdist%5D=0&search%5Bdistrict_id%5D=66&search%5Bsubregion_id%5D=410&search%5Bcity_id%5D=38&nrAdsPerPage=72");
                var foundPages2 = otodom.Search(
                    "https://www.otodom.pl/sprzedaz/mieszkanie/krakow/grzegorzki/?search%5Bfilter_float_price%3Afrom%5D=300000&search%5Bfilter_float_price%3Ato%5D=600000&search%5Bfilter_enum_rooms_num%5D%5B0%5D=3&search%5Bfilter_enum_rooms_num%5D%5B1%5D=4&search%5Bfilter_enum_market%5D%5B0%5D=secondary&search%5Bdescription%5D=1&search%5Bdist%5D=0&search%5Bdistrict_id%5D=58&search%5Bsubregion_id%5D=410&search%5Bcity_id%5D=38&nrAdsPerPage=72");
                var knownPages = await db.Pages.ToListAsync();
                var unknownPages = (await foundPages1)
                    .Union(await foundPages2)
                    .Except(knownPages)
                    .ToList();
                db.Pages.AddRange(unknownPages);
                await db.SaveChangesAsync();

                if (knownPages.Any())
                    foreach (var item in unknownPages)
                        logger.Information($"[{item.Title}]({item.Link})");
            }
        }
    }

    class Config
    {
        // https://github.com/oxozle/serilog-sinks-telegram/issues/1
        public string TelegramApiKey { get; set; }
        public string TelegramChatId { get; set; }
    }

    class AlertsDatabase : DbContext
    {
        public DbSet<Page> Pages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder opt) =>
            opt.UseSqlite("Data Source=Alerts.sqlite3");
    }

    public class Page
    {
        [Key] public string Link { get; set; }
        public string Title { get; set; }
        public override bool Equals(object compared) => string.Equals(Link, ((Page) compared).Link);
        public override int GetHashCode() => Link.GetHashCode();
    }
}