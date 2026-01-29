import { z } from "zod";
import type { TranslationKeys } from "../i18n/translations";

export const createBookSchema = (t: TranslationKeys) =>
  z.object({
    name: z
      .string()
      .min(1, t.validation.nameRequired)
      .max(1000, t.validation.nameMax),
    author: z
      .string()
      .min(1, t.validation.authorRequired)
      .max(1000, t.validation.authorMax),
    isbn: z
      .string()
      .min(1, t.validation.isbnRequired)
      .max(1000, t.validation.isbnMax)
      .regex(
        /^(?:\d{10}|\d{13}|\d{3}-\d{10}|\d{3}-\d-\d{5}-\d{3}-\d)$/,
        t.validation.isbnInvalid,
      ),
    year: z
      .string()
      .min(1, t.validation.yearRequired)
      .refine((val) => {
        const year = parseInt(val);
        return year >= 1000 && year <= new Date().getFullYear();
      }, t.validation.yearInvalid),
    actualQuantity: z
      .string()
      .min(1, t.validation.quantityRequired)
      .refine((val) => {
        const num = parseInt(val);
        return !isNaN(num) && num >= 0;
      }, t.validation.quantityNegative),
  });

export type CreateBookFormData = z.infer<ReturnType<typeof createBookSchema>>;
