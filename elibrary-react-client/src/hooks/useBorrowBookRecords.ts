import { useQuery } from "@tanstack/react-query";
import { getAllBorrowRecords } from "../api";

// Query keys
export const borrowRecordKeys = {
  all: ["borrowRecords"] as const,
};

// Hook for getting all records of borrowning and returning books
export function useBorrowBookRecords() {
  return useQuery({
    queryKey: borrowRecordKeys.all,
    queryFn: getAllBorrowRecords,
  });
}
