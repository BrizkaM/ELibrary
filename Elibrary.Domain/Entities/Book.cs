using System.ComponentModel.DataAnnotations;

namespace ELibrary.Domain.Entities
{
    public class Book
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Name of the book (required)
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Name of the author.
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Year of release.
        /// </summary>
        [Required]
        public DateTime Year { get; set; } = DateTime.MinValue;

        /// <summary>
        /// ISBN.
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// Quantity of books in stock (default: 0)
        /// </summary>
        public int ActualQuantity { get; set; } = 0;

        /// <summary>
        /// Borrow book records associated with this book. Foreign key relationship.
        /// </summary>
        public ICollection<BorrowBookRecord> BorrowBookRecords { get; set; } = new List<BorrowBookRecord>();

        /// <summary>
        /// Row version for concurrency control.
        /// </summary>
        /// <remarks>Manualy handled due to SQLite. Not testes due to SQLite.</remarks>
        public long RowVersion { get; set; }
    }
}
