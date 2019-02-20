using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Gaev.Blog.Examples;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Telegram;

public class CodeFragments
{
    void Serilog_and_Telegram_example()
    {
        var config = new Config();

        var logger = new LoggerConfiguration()
            .WriteTo.Telegram(config.TelegramApiKey, config.TelegramChatId)
            .CreateLogger();
        logger.Information("Offer #1 - https://the.internet/offer-1.html");
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

    async Task EntityFramework_and_Sqlite_example()
    {
        using (var db = new AlertsDatabase())
        {
            await db.Database.EnsureCreatedAsync();
            db.Pages.Add(new Page
            {
                Link = "https://the.internet/offer-1.html",
                Title = "Offer #1"
            });
            await db.SaveChangesAsync();
        }
    }
}