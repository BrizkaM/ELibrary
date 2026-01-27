import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getAllBooks,
  searchBooks,
  createBook,
  borrowBook,
  returnBook,
} from "../api";
import type {
  CreateBookCommand,
  BorrowBookCommand,
  ReturnBookCommand,
  SearchBooksQuery,
} from "../types";

// Query keys - cache management
export const bookKeys = {
  all: ["books"] as const,
  search: (query: SearchBooksQuery) => ["books", "search", query] as const,
};

// Hook for getting all Books
export function useBooks() {
  return useQuery({
    queryKey: bookKeys.all,
    queryFn: getAllBooks,
  });
}

// Hook for searching books
export function useSearchBooks(query: SearchBooksQuery) {
  return useQuery({
    queryKey: bookKeys.search(query),
    queryFn: () => searchBooks(query),
    enabled: !!(query.name || query.author || query.isbn), // runs only if something is filled
  });
}

// Hook for book creation
export function useCreateBook() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: CreateBookCommand) => createBook(command),
    onSuccess: () => {
      // Invalidates cache - reloads list
      queryClient.invalidateQueries({ queryKey: bookKeys.all });
    },
  });
}

// Hook for borrowing book
export function useBorrowBook() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: BorrowBookCommand) => borrowBook(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: bookKeys.all });
    },
  });
}

// Hook for returning book
export function useReturnBook() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: ReturnBookCommand) => returnBook(command),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: bookKeys.all });
    },
  });
}
