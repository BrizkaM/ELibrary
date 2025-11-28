# ELibrary - Library Management System

A simple library management system built with ASP.NET Core Web API and Entity Framework Core, using SQLite as the database and Blazor Client.

## Project
### Brief summary
- IDE used to create project was Visual Studio 2022 (VS2022)
- VS2022 can be used to run all unit tests
- VS2022 can be used to run project itself: press **Start** or **F5** to run project
- Multiple startup projects are set to Run
- Client runs on port `https://localhost:7002/`
- Client runs on port `https://localhost:7001/`
- Swagger can be found on port `https://localhost:7001/swagger`
- Database is created and all migration applied during the start of the project 

## Project Structure

The solution consists of five main projects:

### ELibrary.Shared - created 'manually'
### ELibrary.Database - created 'manually'
### ELibrary.WebApp - created 'manually'
### ELibrary.BlazorClient - Completely AI generated (Claude by Anthropic, Sonnet 4.5), not reviewed
### ELibrary.Tests - Completely AI generated (Claude by Anthropic, Sonnet 4.5), briefly reviewed

README.md file (collab AI and 'manual' work)

## AI usage
- Mostly used Claude (Anthropic, Sonnet 4.5), to create Client side and generate tests
- used for brief consultancy on several topics (mostly row concurrency check, because of SQLite)

## Architecture
- Unit of work pattern ommited due to scale and scope of the project
- RowVersion check handled by trigger (due to SQLite)- this would be different for e.g. MSSQL

## Features

### Book Management
- Get all books
- Search books by name, author, or ISBN
- Create new books with validation
- Unique ISBN constraint

### Borrowing System
- Borrow books (decreases quantity)
- Return books (increases quantity)
- Track all borrow/return operations with customer names and timestamps
- Concurrency control using RowVersion

### Data Validation
- ISBN uniqueness
- Non-negative quantities
- No future dates for book publication year
- Required fields validation testing workflows (client side)

### Test Coverage
- All public methods on controllers
- All public methods on repositories
- All public methods in BookService
- Integration-like tests testing services workflow

## Database

The application uses SQLite with the following tables:

### Books
- ID (Primary Key)
- Name
- Author
- Year
- ISBN (Unique)
- ActualQuantity
- RowVersion (for concurrency control)

### BorrowBookRecords
- ID (Primary Key)
- BookID (Foreign Key)
- CustomerName
- Action (Borrowed/Returned)
- Date

## API Endpoints

### Books
- `GET /Book` - Get all books
- `GET /Book/criteria?name=&author=&isbn=` - Search books
- `POST /Book` - Create a new book
- `POST /Book/borrow?bookId=&customerName=` - Borrow a book
- `POST /Book/return?bookId=&customerName=` - Return a book

### Borrow Records
- `GET /BorrowBookRecord` - Get all borrow/return records

## Prerequisites and Setup

1. **Prerequisites**
   - .NET 8.0 SDK
   - SQLite (included with Entity Framework Core)

2. **Database Setup**
   ```bash
   # The database is automatically created and migrated on application startup
   # Initial seed data includes 3 books by Christopher Ruocchio
   ```
## Seed Data

The application includes initial seed data:
- Empire of Silence (2018) - 3 copies
- Howling Dark (2019) - 3 copies
- Demon in white (2020) - 3 copies

## Technical Highlights

- **Concurrency Control**: Uses RowVersion with SQLite trigger for optimistic concurrency
- **Transaction Management**: Borrow and return operations use database transactions
- **CORS**: Configured for Blazor client access
- **Logging**: Console and debug logging configured
- **Exception Handling**: Global exception handler with appropriate HTTP status codes
- **Clean Architecture**: Separation of concerns with repositories, services, and controllers

## Connection String

Default connection string is configured in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=ELibraryDB.sqlite"
}
```

## Error Handling

The application handles various scenarios:
- Book not found (404)
- Out of stock (400)
- Concurrency conflicts (409)
- Validation errors (400)
- Server errors (500)
