using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.Telegram;

namespace Gaev.Blog.Examples
{
    public class Program
    {
        private const string SpecificOtoDomSearchUrl =
            "https://www.otodom.pl/sprzedaz/mieszkanie/krakow/pradnik-czerwony/?search%5Bfilter_float_price%3Afrom%5D=300000&search%5Bfilter_float_price%3Ato%5D=600000&search%5Bfilter_enum_rooms_num%5D%5B0%5D=3&search%5Bfilter_enum_rooms_num%5D%5B1%5D=4&search%5Bfilter_enum_market%5D%5B0%5D=secondary&search%5Bdescription%5D=1&search%5Bdist%5D=0&search%5Bdistrict_id%5D=66&search%5Bsubregion_id%5D=410&search%5Bcity_id%5D=38&nrAdsPerPage=72";

        public static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .Get<Config>();
            using (var logger = new LoggerConfiguration()
                .WriteTo.Telegram(config.TelegramApiKey, config.TelegramChatId)
                .CreateLogger())
                try
                {
                    using (var db = new AlertsDatabase())
                        await Sync(logger, db);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Oops :(");
                }
        }

        private static async Task Sync(Logger logger, AlertsDatabase db)
        {
            await db.Database.EnsureCreatedAsync();
            var foundPages = await OtoDomCrawler.Search(SpecificOtoDomSearchUrl);
            var knownPages = await db.Pages.ToListAsync();
            var newPages = foundPages.Except(knownPages).ToList();
            db.Pages.AddRange(newPages);
            await db.SaveChangesAsync();
            foreach (var page in newPages)
                logger.Information($"[{page.Title}]({page.Link})");
        }
    }

    public class Config
    {
        public string TelegramApiKey { get; set; }
        public string TelegramChatId { get; set; }
    }

    public class AlertsDatabase : DbContext
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

    public static class OtoDomCrawler
    {
        public static async Task<List<Page>> Search(string url)
        {
            var offers = new List<Page>();
            while (url != null)
            {
                var html = (await new HtmlWeb().LoadFromWebAsync(url)).DocumentNode;
                offers.AddRange(GetOffers(html));
                url = html.SelectSingleNode("//a[@data-dir='next']")?.GetAttributeValue("href", null);
            }

            return offers;
        }

        private static IEnumerable<Page> GetOffers(HtmlNode html)
        {
            foreach (var offer in html.SelectNodes("//*[@class='offer-item-details']"))
            {
                var title = offer.SelectSingleNode(".//*[@class='offer-item-title']");
                var link = title?.AncestorsAndSelf("a").FirstOrDefault();
                yield return new Page
                {
                    Link = link?.GetAttributeValue("href", null)?.Split("#")?.FirstOrDefault(),
                    Title = title?.InnerText
                };
            }
        }
    }
}