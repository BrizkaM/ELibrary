using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELibrary.Shared.Entities
{
    public class BorrowBookRecord
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Unique book identifier
        /// </summary>
        [Required]
        public Guid BookID { get; set; }

        /// <summary>
        /// Customer name (required)
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Action done by customer.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Date of the action.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        [ForeignKey(nameof(BookID))]
        [Required]
        public Book Book { get; set; } = default!;
    }
}
