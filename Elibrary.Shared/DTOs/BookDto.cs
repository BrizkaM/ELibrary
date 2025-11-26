using System.ComponentModel.DataAnnotations;

namespace ELibrary.Shared.DTOs
{
    public class BookDto
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Name of the book (required)
        /// </summary>
        [Required]
        [Display(Name = "Název")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Name of the author.
        /// </summary>
        [Required]
        [Display(Name = "Autor")]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Year of release.
        /// </summary>
        [Required]
        [Display(Name = "Rok vydání")]
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
        [Display(Name = "Počet kusů")]
        public int ActualQuantity { get; set; } = 0;
    }
}
