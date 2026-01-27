// Re-export generovaných typů
export type * from "./api.generated";

// Import pro použití v tomto souboru
import type { components } from "./api.generated";

// Aliasy pro snazší použití
export type Book = components["schemas"]["BookDto"];
export type BorrowBookRecord = components["schemas"]["BorrowBookRecordDto"];
export type CreateBookCommand = components["schemas"]["CreateBookCommand"];
export type BorrowBookCommand = components["schemas"]["BorrowBookCommand"];
export type ReturnBookCommand = components["schemas"]["ReturnBookCommand"];
export type SearchBooksQuery = components["schemas"]["SearchBooksQuery"];

// Vlastní helper typy (tyto ve Swagger nejsou)
export interface ApiError {
  error: string;
  message: string;
  errorCode: string;
}
