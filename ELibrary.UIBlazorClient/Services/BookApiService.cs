using ELibrary.Application.DTOs;

namespace ELibrary.UIBlazorClient.Services
{
    public class BookApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookApiService> _logger;
        private const string ApiPrefix = "api/v1";

        public BookApiService(HttpClient httpClient, ILogger<BookApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<BookDto>> GetAllBooksAsync()
        {
            try
            {
                var books = await _httpClient.GetFromJsonAsync<List<BookDto>>($"{ApiPrefix}/Book");
                return books ?? new List<BookDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při načítání knih");
                return new List<BookDto>();
            }
        }

        public async Task<List<BookDto>> SearchBooksAsync(string? name, string? author, string? isbn)
        {
            try
            {
                // ✅ OPRAVENO: POST s JSON body místo GET s query string
                var query = new SearchBooksQuery
                {
                    Name = name,
                    Author = author,
                    ISBN = isbn
                };

                var response = await _httpClient.PostAsJsonAsync($"{ApiPrefix}/Book/search", query);

                if (response.IsSuccessStatusCode)
                {
                    var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
                    return books ?? new List<BookDto>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new List<BookDto>();
                }
                else
                {
                    _logger.LogWarning("Search failed with status {StatusCode}", response.StatusCode);
                    return new List<BookDto>();
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<BookDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při vyhledávání knih");
                return new List<BookDto>();
            }
        }

        public async Task<(bool Success, string Message)> BorrowBookAsync(Guid bookId, string customerName)
        {
            try
            {
                // ✅ OPRAVENO: PostAsJsonAsync s command object
                var command = new BorrowBookCommand
                {
                    BookId = bookId,
                    CustomerName = string.IsNullOrWhiteSpace(customerName) ? "anonym" : customerName
                };

                var response = await _httpClient.PostAsJsonAsync($"{ApiPrefix}/Book/borrow", command);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Kniha byla úspěšně půjčena.");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return (false, $"Chyba: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při půjčování knihy");
                return (false, $"Chyba: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ReturnBookAsync(Guid bookId, string customerName)
        {
            try
            {
                // ✅ OPRAVENO: PostAsJsonAsync s command object
                var command = new ReturnBookCommand
                {
                    BookId = bookId,
                    CustomerName = string.IsNullOrWhiteSpace(customerName) ? "anonym" : customerName
                };

                var response = await _httpClient.PostAsJsonAsync($"{ApiPrefix}/Book/return", command);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Kniha byla úspěšně vrácena.");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return (false, $"Chyba: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při vracení knihy");
                return (false, $"Chyba: {ex.Message}");
            }
        }

        public async Task<List<BorrowBookRecordDto>> GetAllBorrowBookRecordsAsync()
        {
            try
            {
                var records = await _httpClient.GetFromJsonAsync<List<BorrowBookRecordDto>>($"{ApiPrefix}/BorrowBookRecord");
                return records ?? new List<BorrowBookRecordDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při načítání záznamů o půjčkách");
                return new List<BorrowBookRecordDto>();
            }
        }

        public async Task<(bool Success, string Message)> CreateBookAsync(BookDto bookDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiPrefix}/Book", bookDto);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Kniha byla úspěšně vytvořena.");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return (false, $"Chyba: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při vytváření knihy");
                return (false, $"Chyba: {ex.Message}");
            }
        }
    }

    // ============================================================
    // COMMAND & QUERY DTOs
    // ============================================================

    /// <summary>
    /// Query pro vyhledávání knih (musí odpovídat API)
    /// </summary>
    public class SearchBooksQuery
    {
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? ISBN { get; set; }
    }

    /// <summary>
    /// Command pro půjčení knihy (musí odpovídat API)
    /// </summary>
    public class BorrowBookCommand
    {
        public Guid BookId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command pro vrácení knihy (musí odpovídat API)
    /// </summary>
    public class ReturnBookCommand
    {
        public Guid BookId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }
}