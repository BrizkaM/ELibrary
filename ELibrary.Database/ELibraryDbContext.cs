using ELibrary.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Database;

/// <summary>
/// Database context for E-Shop application
/// </summary>
public class ELibraryDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the EShopDbContext class.
    /// </summary>
    /// <param name="options">The options to be used by this DbContext.</param>
    public ELibraryDbContext(DbContextOptions<ELibraryDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the products database set.
    /// </summary>
    public DbSet<Book> Books { get; set; } = null!;

    /// <summary>
    /// Configures the model and relationships for the e-shop database.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.ID);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Author)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.ISBN)
                .HasPrecision(18, 2);

            entity.Property(e => e.Year)
                .HasMaxLength(2000);

            entity.Property(e => e.ActualQuantity)
                .IsRequired();

            entity.Property(e => e.Idate)
                .IsRequired();
                        
            entity.Property(e => e.Udate)
                .IsRequired();
        });

        // Initial seed
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Seeds the initial product data into the database.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    private void SeedData(ModelBuilder modelBuilder)
    {
        var products = new[]
        {
            new Book
            {
                ID = new Guid(),
                Name = "Empire of Silence",
                Author = "Christopher Ruocchio",
                ISBN = "9780756419264",
                Year = new DateTime(2018, DateTimeKind.Utc),
                ActualQuantity = 3,
                Idate = new DateTime(2025, 11, 21, 24, 0, 0, DateTimeKind.Utc).AddDays(-30),
                Udate = new DateTime(2025, 11, 21, 24, 0, 0, DateTimeKind.Utc).AddDays(-30)
            },
            new Book
            {
                ID = new Guid(),
                Name = "Howling Dark",
                Author = "Christopher Ruocchio",
                ISBN = "9780756419271",
                Year = new DateTime(2019, DateTimeKind.Utc),
                ActualQuantity = 3,
                Idate = new DateTime(2025, 11, 21, 24, 0, 0, DateTimeKind.Utc).AddDays(-30),
                Udate = new DateTime(2025, 11, 21, 24, 0, 0, DateTimeKind.Utc).AddDays(-30)
            },
            new Book
            {
                ID = new Guid(),
                Name = "Demon in white",
                Author = "Christopher Ruocchio",
                ISBN = "9780756419288",
                Year = new DateTime(2020, DateTimeKind.Utc),
                ActualQuantity = 3,
                Idate = new DateTime(2025, 11, 21, 24, 0, 0, DateTimeKind.Utc).AddDays(-30),
                Udate = new DateTime(2025, 11, 21, 24, 0, 0, DateTimeKind.Utc).AddDays(-30)
            },
        };

        modelBuilder.Entity<Book>().HasData(products);
    }
}