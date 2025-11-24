using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELibrary.Shared.Entities
{
    public class Book
    {
        public Book()
        {
            Idate = DateTime.UtcNow;
            Udate = DateTime.UtcNow;
        }

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
        public string ISBN { get; set; }

        /// <summary>
        /// Quantity of books in stock (default: 0)
        /// </summary>
        public int ActualQuantity { get; set; } = 0;

        /// <summary>
        /// Date when the product was created
        /// </summary>
        public DateTime Idate { get; set; }

        /// <summary>
        /// Date when the product was last updated
        /// </summary>
        public DateTime Udate { get; set; }
    }
}
