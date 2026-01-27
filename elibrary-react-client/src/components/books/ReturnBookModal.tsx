import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button, Input, Modal } from "../ui";
import { useReturnBook } from "../../hooks";
import {
  borrowReturnSchema,
  type BorrowReturnFormData,
} from "../../lib/validations";
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

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<BorrowReturnFormData>({
    resolver: zodResolver(borrowReturnSchema),
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

      reset();
      onClose();
    } catch (error) {
      console.error("Chyba při vracení knihy:", error);
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Vrátit knihu">
      <div className="mb-4 p-3 bg-green-50 rounded-lg">
        <p className="text-sm text-green-800">
          <span className="font-medium">Kniha:</span> {book?.name}
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input
          id="customerName"
          label="Jméno zákazníka"
          placeholder="např. Jan Novák"
          error={errors.customerName?.message}
          {...register("customerName")}
        />

        {returnBook.isError && (
          <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-sm text-red-600">Chyba při vracení knihy.</p>
          </div>
        )}

        <div className="flex justify-end gap-3 pt-4">
          <Button type="button" variant="secondary" onClick={handleClose}>
            Zrušit
          </Button>
          <Button
            type="submit"
            isLoading={isSubmitting || returnBook.isPending}
          >
            Vrátit
          </Button>
        </div>
      </form>
    </Modal>
  );
}
