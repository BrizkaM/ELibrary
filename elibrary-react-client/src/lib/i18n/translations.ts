export const translations = {
  cs: {
    // Common
    appName: "E-Library",
    loading: "Načítání...",
    error: "Chyba",
    cancel: "Zrušit",
    save: "Uložit",
    delete: "Smazat",
    searchBtn: "Hledat",
    clear: "Zrušit",

    // Navigation
    nav: {
      home: "Domů",
      books: "Knihy",
      history: "Historie",
    },

    // Home page
    home: {
      title: "Vítejte v E-Library",
      subtitle: "Systém pro správu knihovny - půjčování a vracení knih",
      booksCard: "Knihy",
      booksCardDesc: "Procházejte a spravujte knihy v knihovně",
      addBookCard: "Přidat knihu",
      addBookCardDesc: "Přidejte novou knihu do katalogu",
      historyCard: "Historie",
      historyCardDesc: "Zobrazit historii půjček a vrácení",
    },

    // Books page
    books: {
      title: "Knihy",
      totalBooks: "Celkem {count} knih v knihovně",
      foundBooks: "Nalezeno {count} knih",
      addBook: "Přidat knihu",
      noBooks: "Žádné knihy v knihovně",
      noBooksFound: "Žádné knihy neodpovídají vyhledávání",
      author: "Autor",
      isbn: "ISBN",
      year: "Rok",
      inStock: "Skladem",
      pcs: "ks",
      borrow: "Půjčit",
      return: "Vrátit",
      loadError: "Chyba při načítání knih",
    },

    // Search
    search: {
      placeholder: "Hledat podle názvu...",
      authorPlaceholder: "Hledat podle autora...",
      isbnPlaceholder: "Hledat podle ISBN...",
      activeFilters: "Aktivní filtry",
      name: "Název",
    },

    // Create book modal
    createBook: {
      title: "Přidat novou knihu",
      nameLabel: "Název knihy",
      namePlaceholder: "např. Harry Potter",
      authorLabel: "Autor",
      authorPlaceholder: "např. J.K. Rowling",
      isbnLabel: "ISBN",
      isbnPlaceholder: "např. 9780756419264",
      yearLabel: "Rok vydání",
      yearPlaceholder: "např. 2020",
      quantityLabel: "Počet kusů",
      submit: "Vytvořit knihu",
      success: 'Kniha "{name}" byla úspěšně vytvořena',
      error:
        "Chyba při vytváření knihy. Zkontrolujte, zda ISBN již neexistuje.",
    },

    // Borrow modal
    borrowBook: {
      title: "Půjčit knihu",
      bookLabel: "Kniha",
      inStockLabel: "Skladem",
      customerLabel: "Jméno zákazníka",
      customerPlaceholder: "např. Jan Novák",
      submit: "Půjčit",
      success: 'Kniha "{book}" byla půjčena zákazníkovi {customer}',
      error: "Chyba při půjčování knihy. Kniha možná není skladem.",
    },

    // Return modal
    returnBook: {
      title: "Vrátit knihu",
      bookLabel: "Kniha",
      customerLabel: "Jméno zákazníka",
      customerPlaceholder: "např. Jan Novák",
      submit: "Vrátit",
      success: 'Kniha "{book}" byla vrácena zákazníkem {customer}',
      error: "Chyba při vracení knihy.",
    },

    // History page
    history: {
      title: "Historie půjček",
      totalRecords: "Celkem {count} záznamů",
      records: "Záznamy",
      noRecords: "Žádné záznamy o půjčkách",
      borrowed: "Půjčil/a si knihu",
      returned: "Vrátil/a knihu",
      loadError: "Chyba při načítání historie",
    },

    // Validation
    validation: {
      required: "Toto pole je povinné",
      nameRequired: "Název knihy je povinný",
      nameMax: "Název může mít maximálně 1000 znaků",
      authorRequired: "Autor je povinný",
      authorMax: "Autor může mít maximálně 1000 znaků",
      isbnRequired: "ISBN je povinné",
      isbnMax: "ISBN může mít maximálně 1000 znaků",
      isbnInvalid: "Neplatný formát ISBN (očekáván ISBN-10 nebo ISBN-13)",
      yearRequired: "Rok vydání je povinný",
      yearInvalid: "Rok musí být mezi 1000 a aktuálním rokem",
      quantityRequired: "Množství je povinné",
      quantityNegative: "Množství musí být nezáporné číslo",
      customerMin: "Jméno musí mít alespoň 2 znaky",
      customerMax: "Jméno může mít maximálně 1000 znaků",
      customerInvalid: "Jméno obsahuje neplatné znaky",
    },

    // Theme
    theme: {
      light: "Přepnout na tmavý režim",
      dark: "Přepnout na světlý režim",
    },

    // 404 page
    notFound: {
      title: "Stránka nenalezena",
      message: "Omlouváme se, ale stránka kterou hledáte neexistuje.",
      backHome: "Zpět na hlavní stránku",
    },

    // Errors
    errors: {
      unknown: "Nastala neočekávaná chyba",
      networkError: "Chyba připojení k serveru",
      serverError: "Chyba serveru, zkuste to později",
      badRequest: "Neplatný požadavek",
      notFound: "Záznam nebyl nalezen",
      conflict: "Konflikt dat",
      duplicateIsbn: "Kniha s tímto ISBN již existuje",
      outOfStock: "Kniha není skladem",
      concurrencyConflict:
        "Kniha byla změněna jiným uživatelem, zkuste to znovu",
      validation: "Chyba validace dat",
    },
  },

  en: {
    // Common
    appName: "E-Library",
    loading: "Loading...",
    error: "Error",
    cancel: "Cancel",
    save: "Save",
    delete: "Delete",
    searchBtn: "Search",
    clear: "Clear",

    // Navigation
    nav: {
      home: "Home",
      books: "Books",
      history: "History",
    },

    // Home page
    home: {
      title: "Welcome to E-Library",
      subtitle: "Library management system - borrowing and returning books",
      booksCard: "Books",
      booksCardDesc: "Browse and manage books in the library",
      addBookCard: "Add Book",
      addBookCardDesc: "Add a new book to the catalog",
      historyCard: "History",
      historyCardDesc: "View borrowing and return history",
    },

    // Books page
    books: {
      title: "Books",
      totalBooks: "Total {count} books in library",
      foundBooks: "Found {count} books",
      addBook: "Add Book",
      noBooks: "No books in library",
      noBooksFound: "No books match your search",
      author: "Author",
      isbn: "ISBN",
      year: "Year",
      inStock: "In Stock",
      pcs: "pcs",
      borrow: "Borrow",
      return: "Return",
      loadError: "Error loading books",
    },

    // Search
    search: {
      placeholder: "Search by name...",
      authorPlaceholder: "Search by author...",
      isbnPlaceholder: "Search by ISBN...",
      activeFilters: "Active filters",
      name: "Name",
    },

    // Create book modal
    createBook: {
      title: "Add New Book",
      nameLabel: "Book Name",
      namePlaceholder: "e.g. Harry Potter",
      authorLabel: "Author",
      authorPlaceholder: "e.g. J.K. Rowling",
      isbnLabel: "ISBN",
      isbnPlaceholder: "e.g. 9780756419264",
      yearLabel: "Publication Year",
      yearPlaceholder: "e.g. 2020",
      quantityLabel: "Quantity",
      submit: "Create Book",
      success: 'Book "{name}" was successfully created',
      error: "Error creating book. Please check if ISBN already exists.",
    },

    // Borrow modal
    borrowBook: {
      title: "Borrow Book",
      bookLabel: "Book",
      inStockLabel: "In Stock",
      customerLabel: "Customer Name",
      customerPlaceholder: "e.g. John Smith",
      submit: "Borrow",
      success: 'Book "{book}" was borrowed by {customer}',
      error: "Error borrowing book. Book may be out of stock.",
    },

    // Return modal
    returnBook: {
      title: "Return Book",
      bookLabel: "Book",
      customerLabel: "Customer Name",
      customerPlaceholder: "e.g. John Smith",
      submit: "Return",
      success: 'Book "{book}" was returned by {customer}',
      error: "Error returning book.",
    },

    // History page
    history: {
      title: "Borrow History",
      totalRecords: "Total {count} records",
      records: "Records",
      noRecords: "No borrow records",
      borrowed: "Borrowed a book",
      returned: "Returned a book",
      loadError: "Error loading history",
    },

    // Validation
    validation: {
      required: "This field is required",
      nameRequired: "Book name is required",
      nameMax: "Name cannot exceed 1000 characters",
      authorRequired: "Author is required",
      authorMax: "Author cannot exceed 1000 characters",
      isbnRequired: "ISBN is required",
      isbnMax: "ISBN cannot exceed 1000 characters",
      isbnInvalid: "Invalid ISBN format (expected ISBN-10 or ISBN-13)",
      yearRequired: "Publication year is required",
      yearInvalid: "Year must be between 1000 and current year",
      quantityRequired: "Quantity is required",
      quantityNegative: "Quantity must be a non-negative number",
      customerMin: "Name must be at least 2 characters",
      customerMax: "Name cannot exceed 1000 characters",
      customerInvalid: "Name contains invalid characters",
    },

    // Theme
    theme: {
      light: "Switch to dark mode",
      dark: "Switch to light mode",
    },

    // 404 page
    notFound: {
      title: "Page Not Found",
      message: "Sorry, the page you are looking for does not exist.",
      backHome: "Back to Home",
    },

    // Errors
    errors: {
      unknown: "An unexpected error occurred",
      networkError: "Unable to connect to server",
      serverError: "Server error, please try again later",
      badRequest: "Invalid request",
      notFound: "Record not found",
      conflict: "Data conflict",
      duplicateIsbn: "A book with this ISBN already exists",
      outOfStock: "Book is out of stock",
      concurrencyConflict:
        "Book was modified by another user, please try again",
      validation: "Validation error",
    },
  },
} as const;

export type Language = keyof typeof translations;
export type TranslationKeys = typeof translations.cs;
