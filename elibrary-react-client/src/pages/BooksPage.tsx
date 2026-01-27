import { useState } from "react";
import { useBooks } from "../hooks";
import {
  Card,
  CardContent,
  CardHeader,
  CardFooter,
  Button,
  CreateBookModal,
  BorrowBookModal,
  ReturnBookModal,
} from "../components";
import {
  BookOpen,
  AlertCircle,
  Plus,
  ArrowUpRight,
  ArrowDownLeft,
} from "lucide-react";
import type { Book } from "../types";

export function BooksPage() {
  const { data: books, isLoading, error } = useBooks();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [borrowingBook, setBorrowingBook] = useState<Book | null>(null);
  const [returningBook, setReturningBook] = useState<Book | null>(null);

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
          <span>Chyba při načítání knih: {error.message}</span>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-8 flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Knihy</h1>
          <p className="mt-2 text-gray-600">
            Celkem {books?.length || 0} knih v knihovně
          </p>
        </div>
        <Button onClick={() => setIsCreateModalOpen(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Přidat knihu
        </Button>
      </div>

      {books && books.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {books.map((book: Book) => (
            <Card key={book.id}>
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-3">
                    <BookOpen className="h-6 w-6 text-blue-600" />
                    <h3 className="font-semibold text-gray-900 line-clamp-1">
                      {book.name}
                    </h3>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-2 text-sm">
                  <p className="text-gray-600">
                    <span className="font-medium">Autor:</span> {book.author}
                  </p>
                  <p className="text-gray-600">
                    <span className="font-medium">ISBN:</span> {book.isbn}
                  </p>
                  <p className="text-gray-600">
                    <span className="font-medium">Rok:</span>{" "}
                    {new Date(book.year ?? "").getFullYear()}
                  </p>
                  <p
                    className={
                      (book.actualQuantity ?? 0) > 0
                        ? "text-green-600"
                        : "text-red-600"
                    }
                  >
                    <span className="font-medium">Skladem:</span>{" "}
                    {book.actualQuantity ?? 0} ks
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
                  Půjčit
                </Button>
                <Button
                  size="sm"
                  variant="secondary"
                  onClick={() => setReturningBook(book)}
                  className="flex-1"
                >
                  <ArrowDownLeft className="h-4 w-4 mr-1" />
                  Vrátit
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <CardContent className="py-12 text-center">
            <BookOpen className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-600">Žádné knihy v knihovně</p>
          </CardContent>
        </Card>
      )}

      {/* Modaly */}
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
