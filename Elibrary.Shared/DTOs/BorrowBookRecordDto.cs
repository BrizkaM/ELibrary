using System.ComponentModel.DataAnnotations;

namespace ELibrary.Shared.DTOs
{
    /// <summary>
    /// Borrow Book Record Data Transfer Object for web API communication.
    /// </summary>
    public class BorrowBookRecordDto
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Unique book identifier
        /// </summary>
        [Required]
        [Display(Name = "ID knihy")]
        public Guid BookID { get; set; }

        /// <summary>
        /// Customer name (required)
        /// </summary>
        [Required]
        [Display(Name = "Jméno zákazníka")]
        [MaxLength(1000)]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Action done by customer.
        /// </summary>
        [Required]
        [Display(Name = "Akce zákazníka")]
        [MaxLength(200)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Date of the action.
        /// </summary>
        [Required]
        [Display(Name = "Datum změny")]
        public DateTime Date { get; set; }
    }
}
