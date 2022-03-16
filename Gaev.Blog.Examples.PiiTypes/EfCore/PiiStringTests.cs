using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Gaev.Blog.Examples.EfCore;

public class PiiStringTests
{
    [Test]
    public async Task EfCore_should_work()
    {
        // Given
        await using var db = new TestDbContext(new PiiAsPlainText());
        await db.Database.EnsureCreatedAsync();
        var givenUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john.doe@test.com"
        };

        // When
        db.Users.Add(givenUser);
        await db.SaveChangesAsync();
        db.DetachAll();
        var savedUser = await db.Users.FindAsync(givenUser.Id);

        // Then
        savedUser.Should().BeEquivalentTo(givenUser);
    }

    [Test]
    public async Task EfCore_queries_should_work()
    {
        // Given
        await using var db = new TestDbContext(new PiiAsPlainText());
        await db.Database.EnsureCreatedAsync();
        var givenUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john.doe@test.com"
        };

        // When
        db.Users.Add(givenUser);
        await db.SaveChangesAsync();
        db.DetachAll();
        var savedUser = await db.Users
            .Where(e => ((string) e.Email).Contains("john.doe@test.com") || e.Name == "bla")
            .ToListAsync();
    }

    [Test]
    public async Task EfCore_should_encrypt()
    {
        // Given
        var key = "hb50qBZSF0fcLSl9814PIqmO4gEZcJGB/Kd4fpTTBcU=";
        await using var db = new TestDbContext(new PiiAsAes128(key));
        await db.Database.EnsureCreatedAsync();
        var givenUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john.doe@test.com"
        };

        // When
        db.Users.Add(givenUser);
        await db.SaveChangesAsync();
        db.DetachAll();
        var savedUser = await db.Users.FindAsync(givenUser.Id);

        // Then
        savedUser.Should().BeEquivalentTo(givenUser);
    }
}

public class TestDbContext : DbContext
{
    private readonly IPiiEncoder _piiEncoder;

    public TestDbContext(IPiiEncoder piiEncoder) : this(
        "Server=localhost;Port=5432;Database=playground;User ID=postgres;Password=sa123;")
    {
        _piiEncoder = piiEncoder;
    }

    public TestDbContext(string connectionString)
        : base(new DbContextOptionsBuilder().UseNpgsql(connectionString).Options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var piiConverter = new PiiStringConverter(_piiEncoder);

        modelBuilder.Entity<User>(opt =>
        {
            opt.ToTable("User");
            opt.Property(e => e.Name).HasConversion(piiConverter);
            opt.Property(e => e.Email).HasConversion(piiConverter);
            opt.Property(e => e.IAmNull).HasConversion(piiConverter);
        });
    }

    public DbSet<User> Users { get; set; }
}

public static class DbContextEfCoreExt
{
    public static void DetachAll(this DbContext db)
    {
        foreach (var dbEntityEntry in db.ChangeTracker.Entries().ToList())
        {
            if (dbEntityEntry.Entity != null)
            {
                dbEntityEntry.State = EntityState.Detached;
            }
        }
    }
}