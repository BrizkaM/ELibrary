import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMemo } from "react";
import { Button, Input, Modal } from "../ui";
import { useBorrowBook } from "../../hooks";
import { useToast } from "../../store/toastStore";
import { useTranslation } from "../../store/languageStore";
import {
  borrowReturnSchema,
  type BorrowReturnFormData,
} from "../../lib/validations";
import { getErrorMessage } from "../../lib/errorHandler";
import type { Book } from "../../types";

interface BorrowBookModalProps {
  isOpen: boolean;
  onClose: () => void;
  book: Book | null;
}

export function BorrowBookModal({
  isOpen,
  onClose,
  book,
}: BorrowBookModalProps) {
  const borrowBook = useBorrowBook();
  const toast = useToast();
  const { t, translate } = useTranslation();

  const schema = useMemo(() => borrowReturnSchema(t), [t]);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<BorrowReturnFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      customerName: "",
    },
  });

  const onSubmit = async (data: BorrowReturnFormData) => {
    if (!book?.id) return;

    try {
      await borrowBook.mutateAsync({
        bookId: book.id,
        customerName: data.customerName,
      });

      toast.success(
        translate(t.borrowBook.success, {
          book: book.name ?? "",
          customer: data.customerName,
        }),
      );
      reset();
      onClose();
    } catch (error) {
      toast.error(getErrorMessage(error, t));
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title={t.borrowBook.title}>
      <div className="mb-4 p-3 bg-blue-50 dark:bg-blue-900/30 rounded-lg">
        <p className="text-sm text-blue-800 dark:text-blue-200">
          <span className="font-medium">{t.borrowBook.bookLabel}:</span>{" "}
          {book?.name}
        </p>
        <p className="text-sm text-blue-800 dark:text-blue-200">
          <span className="font-medium">{t.borrowBook.inStockLabel}:</span>{" "}
          {book?.actualQuantity ?? 0} {t.books.pcs}
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input
          id="customerName"
          label={t.borrowBook.customerLabel}
          placeholder={t.borrowBook.customerPlaceholder}
          error={errors.customerName?.message}
          {...register("customerName")}
        />

        <div className="flex justify-end gap-3 pt-4">
          <Button type="button" variant="secondary" onClick={handleClose}>
            {t.cancel}
          </Button>
          <Button
            type="submit"
            isLoading={isSubmitting || borrowBook.isPending}
            disabled={(book?.actualQuantity ?? 0) <= 0}
          >
            {t.borrowBook.submit}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
