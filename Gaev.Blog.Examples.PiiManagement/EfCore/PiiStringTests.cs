using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Gaev.Blog.Examples.PiiManagement.EfCore;

public class PiiStringTests
{
    [Test]
    public async Task EfCore_should_work()
    {
        // Given
        await using var db = new TestDbContext();
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
    public async Task EfCore_should_encrypt()
    {
        // Given
        using var _ = new PiiScope(new PiiSerializers.Aes128("hb50qBZSF0fcLSl9814PIqmO4gEZcJGB/Kd4fpTTBcU="));
        await using var db = new TestDbContext();
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
    public TestDbContext() : this("server=localhost;database=PiiPlayground;UID=sa;PWD=sa123")
    {
    }

    public TestDbContext(string connectionString)
        : base(new DbContextOptionsBuilder().UseSqlServer(connectionString).Options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var piiConverter = new PiiStringConverter();

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