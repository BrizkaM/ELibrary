import { z } from "zod";

// Schéma pro vytvoření knihy
export const createBookSchema = z.object({
  name: z
    .string()
    .min(1, "Název knihy je povinný")
    .max(1000, "Název může mít maximálně 1000 znaků"),
  author: z
    .string()
    .min(1, "Autor je povinný")
    .max(1000, "Autor může mít maximálně 1000 znaků"),
  isbn: z
    .string()
    .min(1, "ISBN je povinné")
    .max(1000, "ISBN může mít maximálně 1000 znaků")
    .regex(
      /^(?:\d{10}|\d{13}|\d{3}-\d{10}|\d{3}-\d-\d{5}-\d{3}-\d)$/,
      "Neplatný formát ISBN (očekáván ISBN-10 nebo ISBN-13)",
    ),
  year: z
    .string()
    .min(1, "Rok vydání je povinný")
    .refine((val) => {
      const year = parseInt(val);
      return year >= 1000 && year <= new Date().getFullYear();
    }, "Rok musí být mezi 1000 a aktuálním rokem"),
  actualQuantity: z.coerce.number().min(0, "Množství nemůže být záporné"),
});

// TypeScript typ odvozený ze schématu
export type CreateBookFormData = z.infer<typeof createBookSchema>;

// Schéma pro půjčení/vrácení knihy
export const borrowReturnSchema = z.object({
  customerName: z
    .string()
    .min(2, "Jméno musí mít alespoň 2 znaky")
    .max(1000, "Jméno může mít maximálně 1000 znaků")
    .regex(
      /^[a-zA-ZáčďéěíňóřšťúůýžÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ\s\-'.]+$/,
      "Jméno obsahuje neplatné znaky",
    ),
});

export type BorrowReturnFormData = z.infer<typeof borrowReturnSchema>;
