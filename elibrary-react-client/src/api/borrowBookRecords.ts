import { apiClient } from "./client";
import type { BorrowBookRecord } from "../types";

// GET /borrowbookrecord - Získat všechny záznamy o půjčkách
export async function getAllBorrowRecords(): Promise<BorrowBookRecord[]> {
  const response = await apiClient.get<BorrowBookRecord[]>("/borrowbookrecord");
  return response.data;
}
