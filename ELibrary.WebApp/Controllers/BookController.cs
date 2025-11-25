using ELibrary.Shared.DTOs;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BookController> _logger;

        public BookController(IBookRepository bookRepository, ILogger<BookController> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            var books = await _bookRepository.GetAllAsync();
            return Ok(books);
        }

        [HttpGet("criteria")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Book>>> FindBooksByCriteria(string? name, string? author, string? iban)
        {
            var books = (await _bookRepository.GetFilteredBooksAsync(name, author, iban)).Select(MapToDto);

            if (!books.Any())
            {
                return NotFound();
            }

            return Ok(books);
        }

        private BookDto MapToDto(Book book)
        {
            return new BookDto
            {
                ID = book.ID,
                Name = book.Name,
                Author = book.Author,
                Year = book.Year,
                ISBN = book.ISBN,
                ActualQuantity = book.ActualQuantity
            };
        }
    }
}
