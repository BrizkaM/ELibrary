using ELibrary.Shared.DTOs;
using System.Net.Http.Json;

namespace ELibrary.BlazorClient.Services
{
    public class BookApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookApiService> _logger;

        public BookApiService(HttpClient httpClient, ILogger<BookApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<BookDto>> GetAllBooksAsync()
        {
            try
            {
                var books = await _httpClient.GetFromJsonAsync<List<BookDto>>("Book");
                return books ?? new List<BookDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při načítání knih");
                return new List<BookDto>();
            }
        }

        public async Task<List<BookDto>> FindBooksByCriteriaAsync(string? name, string? author, string? isbn)
        {
            try
            {
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(name))
                    query.Add($"name={Uri.EscapeDataString(name)}");
                if (!string.IsNullOrWhiteSpace(author))
                    query.Add($"author={Uri.EscapeDataString(author)}");
                if (!string.IsNullOrWhiteSpace(isbn))
                    query.Add($"isbn={Uri.EscapeDataString(isbn)}");

                var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
                var books = await _httpClient.GetFromJsonAsync<List<BookDto>>($"Book/criteria{queryString}");
                return books ?? new List<BookDto>();
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

        public async Task<(bool Success, string Message)> BorrowBookAsync(Guid bookId, string? customerName)
        {
            try
            {
                var query = $"?bookId={bookId}";
                if (!string.IsNullOrWhiteSpace(customerName))
                    query += $"&customerName={Uri.EscapeDataString(customerName)}";

                var response = await _httpClient.PatchAsync($"Book/borrow{query}", null);
                
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

        public async Task<(bool Success, string Message)> ReturnBookAsync(Guid bookId, string? customerName)
        {
            try
            {
                var query = $"?bookId={bookId}";
                if (!string.IsNullOrWhiteSpace(customerName))
                    query += $"&customerName={Uri.EscapeDataString(customerName)}";

                var response = await _httpClient.PatchAsync($"Book/return{query}", null);
                
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
                var records = await _httpClient.GetFromJsonAsync<List<BorrowBookRecordDto>>("BorrowBookRecord");
                return records ?? new List<BorrowBookRecordDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při načítání záznamů o půjčkách");
                return new List<BorrowBookRecordDto>();
            }
        }
    }
}
