import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { useBooks, useSearchBooks } from "../hooks";
import {
  Card,
  CardContent,
  CardHeader,
  CardFooter,
  Button,
  CreateBookModal,
  BorrowBookModal,
  ReturnBookModal,
  BookSearch,
} from "../components";
import {
  BookOpen,
  AlertCircle,
  Plus,
  ArrowUpRight,
  ArrowDownLeft,
} from "lucide-react";
import type { Book, SearchBooksQuery } from "../types";
import { useTranslation } from "../store/languageStore";

export function BooksPage() {
  const { t, translate } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [borrowingBook, setBorrowingBook] = useState<Book | null>(null);
  const [returningBook, setReturningBook] = useState<Book | null>(null);
  const [searchQuery, setSearchQuery] = useState<SearchBooksQuery | null>(null);

  // Zachytí ?action=create z URL a otevře modal
  useEffect(() => {
    if (searchParams.get("action") === "create") {
      setIsCreateModalOpen(true);
      // Odstraní query param z URL
      setSearchParams({});
    }
  }, [searchParams, setSearchParams]);

  const allBooksQuery = useBooks();
  const searchBooksQuery = useSearchBooks(
    searchQuery ?? { name: "", author: "", isbn: "" },
  );

  const isSearching = searchQuery !== null;
  const {
    data: books,
    isLoading,
    error,
  } = isSearching ? searchBooksQuery : allBooksQuery;

  const handleSearch = (query: SearchBooksQuery) => {
    setSearchQuery(query);
  };

  const handleClearSearch = () => {
    setSearchQuery(null);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="flex items-center gap-2 text-red-600">
          <AlertCircle className="h-5 w-5" />
          <span>
            {t.books.loadError}: {error.message}
          </span>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-8 flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            {t.books.title}
          </h1>
          <p className="mt-2 text-gray-600 dark:text-gray-400">
            {isSearching
              ? translate(t.books.foundBooks, { count: books?.length || 0 })
              : translate(t.books.totalBooks, { count: books?.length || 0 })}
          </p>
        </div>
        <Button onClick={() => setIsCreateModalOpen(true)}>
          <Plus className="h-4 w-4 mr-2" />
          {t.books.addBook}
        </Button>
      </div>

      <BookSearch
        onSearch={handleSearch}
        onClear={handleClearSearch}
        isSearching={isSearching}
        activeQuery={searchQuery}
      />

      {books && books.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {books.map((book: Book) => (
            <Card key={book.id}>
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-3">
                    <BookOpen className="h-6 w-6 text-blue-600" />
                    <h3 className="font-semibold text-gray-900 dark:text-white line-clamp-1">
                      {book.name}
                    </h3>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-2 text-sm">
                  <p className="text-gray-600 dark:text-gray-400">
                    <span className="font-medium">{t.books.author}:</span>{" "}
                    {book.author}
                  </p>
                  <p className="text-gray-600 dark:text-gray-400">
                    <span className="font-medium">{t.books.isbn}:</span>{" "}
                    {book.isbn}
                  </p>
                  <p className="text-gray-600 dark:text-gray-400">
                    <span className="font-medium">{t.books.year}:</span>{" "}
                    {new Date(book.year ?? "").getFullYear()}
                  </p>
                  <p
                    className={
                      (book.actualQuantity ?? 0) > 0
                        ? "text-green-600"
                        : "text-red-600"
                    }
                  >
                    <span className="font-medium">{t.books.inStock}:</span>{" "}
                    {book.actualQuantity ?? 0} {t.books.pcs}
                  </p>
                </div>
              </CardContent>
              <CardFooter className="flex gap-2">
                <Button
                  size="sm"
                  variant="primary"
                  onClick={() => setBorrowingBook(book)}
                  disabled={(book.actualQuantity ?? 0) <= 0}
                  className="flex-1"
                >
                  <ArrowUpRight className="h-4 w-4 mr-1" />
                  {t.books.borrow}
                </Button>
                <Button
                  size="sm"
                  variant="secondary"
                  onClick={() => setReturningBook(book)}
                  className="flex-1"
                >
                  <ArrowDownLeft className="h-4 w-4 mr-1" />
                  {t.books.return}
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <CardContent className="py-12 text-center">
            <BookOpen className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-600 dark:text-gray-400">
              {isSearching ? t.books.noBooksFound : t.books.noBooks}
            </p>
          </CardContent>
        </Card>
      )}

      <CreateBookModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
      />
      <BorrowBookModal
        isOpen={borrowingBook !== null}
        onClose={() => setBorrowingBook(null)}
        book={borrowingBook}
      />
      <ReturnBookModal
        isOpen={returningBook !== null}
        onClose={() => setReturningBook(null)}
        book={returningBook}
      />
    </div>
  );
}
