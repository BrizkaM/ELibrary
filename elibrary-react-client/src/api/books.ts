import { apiClient } from "./client";
import type {
  Book,
  CreateBookCommand,
  BorrowBookCommand,
  ReturnBookCommand,
  SearchBooksQuery,
} from "../types";

// GET /book
export async function getAllBooks(): Promise<Book[]> {
  const response = await apiClient.get<Book[]>("/book");
  return response.data;
}

// POST /book/search
export async function searchBooks(query: SearchBooksQuery): Promise<Book[]> {
  const response = await apiClient.post<Book[]>("/book/search", query);
  return response.data;
}

// POST /book
export async function createBook(command: CreateBookCommand): Promise<Book> {
  const response = await apiClient.post<Book>("/book", command);
  return response.data;
}

// POST /book/borrow
export async function borrowBook(command: BorrowBookCommand): Promise<Book> {
  const response = await apiClient.post<Book>("/book/borrow", command);
  return response.data;
}

// POST /book/return
export async function returnBook(command: ReturnBookCommand): Promise<Book> {
  const response = await apiClient.post<Book>("/book/return", command);
  return response.data;
}
