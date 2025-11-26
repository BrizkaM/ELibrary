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
    /// Gets or sets the Books database set.
    /// </summary>
    public DbSet<Book> Books { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Borrow-Book-Records database set.
    /// </summary>
    public DbSet<BorrowBookRecord> BorrowBookRecords { get; set; } = null!;

    /// <summary>
    /// Configures the model and relationships for the e-shop database.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure book entity
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
                .HasMaxLength(1000);

            entity.Property(e => e.Year)
                .IsRequired();

            entity.Property(e => e.ActualQuantity)
                .IsRequired();

            entity.Property(e => e.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnUpdate()
                .HasDefaultValue(0L);
        });

        // Configure borrow book record entity
        modelBuilder.Entity<BorrowBookRecord>(entity =>
        {
            entity.HasKey(e => e.ID);

            entity.Property(e => e.BookID)
                .IsRequired();

            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Date)
                .IsRequired();

            entity.HasOne(bbr => bbr.Book)
                .WithMany(b => b.BorrowBookRecords)
                .HasForeignKey(bbr => bbr.BookID)
                .OnDelete(DeleteBehavior.Restrict)
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
                ID = new Guid("c3984d72-57a4-432d-88b1-38290f93450e"),
                Name = "Empire of Silence",
                Author = "Christopher Ruocchio",
                ISBN = "9780756419264",
                Year = new DateTime(2018, 1, 1, 1, 0, 0, DateTimeKind.Utc),
                ActualQuantity = 3,
            },
            new Book
            {
                ID = new Guid("c3984d72-57a4-432d-88b1-38290f93450a"),
                Name = "Howling Dark",
                Author = "Christopher Ruocchio",
                ISBN = "9780756419271",
                Year = new DateTime(2019, 1, 1, 1, 0, 0, DateTimeKind.Utc),
                ActualQuantity = 3,
            },
            new Book
            {
                ID = new Guid("c3984d72-57a4-432d-88b1-38290f93450b"),
                Name = "Demon in white",
                Author = "Christopher Ruocchio",
                ISBN = "9780756419288",
                Year = new DateTime(2020, 11, 24, 1, 0, 0, DateTimeKind.Utc),
                ActualQuantity = 3,
            },
        };

        modelBuilder.Entity<Book>().HasData(products);
    }
}