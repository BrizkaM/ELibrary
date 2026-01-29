import { z } from "zod";
import type { TranslationKeys } from "../i18n/translations";

export const borrowReturnSchema = (t: TranslationKeys) =>
  z.object({
    customerName: z
      .string()
      .min(2, t.validation.customerMin)
      .max(1000, t.validation.customerMax)
      .regex(
        /^[a-zA-ZáčďéěíňóřšťúůýžÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ\s\-'.]+$/,
        t.validation.customerInvalid,
      ),
  });

export type BorrowReturnFormData = z.infer<
  ReturnType<typeof borrowReturnSchema>
>;
