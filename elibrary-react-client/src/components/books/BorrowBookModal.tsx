import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button, Input, Modal } from "../ui";
import { useBorrowBook } from "../../hooks";
import {
  borrowReturnSchema,
  type BorrowReturnFormData,
} from "../../lib/validations";
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
      await borrowBook.mutateAsync({
        bookId: book.id,
        customerName: data.customerName,
      });

      reset();
      onClose();
    } catch (error) {
      console.error("Chyba při půjčování knihy:", error);
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Půjčit knihu">
      <div className="mb-4 p-3 bg-blue-50 rounded-lg">
        <p className="text-sm text-blue-800">
          <span className="font-medium">Kniha:</span> {book?.name}
        </p>
        <p className="text-sm text-blue-800">
          <span className="font-medium">Skladem:</span>{" "}
          {book?.actualQuantity ?? 0} ks
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

        {borrowBook.isError && (
          <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-sm text-red-600">
              Chyba při půjčování knihy. Kniha možná není skladem.
            </p>
          </div>
        )}

        <div className="flex justify-end gap-3 pt-4">
          <Button type="button" variant="secondary" onClick={handleClose}>
            Zrušit
          </Button>
          <Button
            type="submit"
            isLoading={isSubmitting || borrowBook.isPending}
            disabled={(book?.actualQuantity ?? 0) <= 0}
          >
            Půjčit
          </Button>
        </div>
      </form>
    </Modal>
  );
}
