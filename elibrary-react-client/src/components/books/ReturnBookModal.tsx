import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMemo } from "react";
import { Button, Input, Modal } from "../ui";
import { useReturnBook } from "../../hooks";
import { useToast } from "../../store/toastStore";
import { useTranslation } from "../../store/languageStore";
import {
  borrowReturnSchema,
  type BorrowReturnFormData,
} from "../../lib/validations";
import { getErrorMessage } from "../../lib/errorHandler";
import type { Book } from "../../types";

interface ReturnBookModalProps {
  isOpen: boolean;
  onClose: () => void;
  book: Book | null;
}

export function ReturnBookModal({
  isOpen,
  onClose,
  book,
}: ReturnBookModalProps) {
  const returnBook = useReturnBook();
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
      await returnBook.mutateAsync({
        bookId: book.id,
        customerName: data.customerName,
      });

      toast.success(
        translate(t.returnBook.success, {
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
    <Modal isOpen={isOpen} onClose={handleClose} title={t.returnBook.title}>
      <div className="mb-4 p-3 bg-green-50 dark:bg-green-900/30 rounded-lg">
        <p className="text-sm text-green-800 dark:text-green-200">
          <span className="font-medium">{t.returnBook.bookLabel}:</span>{" "}
          {book?.name}
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input
          id="customerName"
          label={t.returnBook.customerLabel}
          placeholder={t.returnBook.customerPlaceholder}
          error={errors.customerName?.message}
          {...register("customerName")}
        />

        <div className="flex justify-end gap-3 pt-4">
          <Button type="button" variant="secondary" onClick={handleClose}>
            {t.cancel}
          </Button>
          <Button
            type="submit"
            isLoading={isSubmitting || returnBook.isPending}
          >
            {t.returnBook.submit}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
