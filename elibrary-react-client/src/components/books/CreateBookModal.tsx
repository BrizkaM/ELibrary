import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button, Input, Modal } from "../ui";
import { useCreateBook } from "../../hooks";
import {
  createBookSchema,
  type CreateBookFormData,
} from "../../lib/validations";

interface CreateBookModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CreateBookModal({ isOpen, onClose }: CreateBookModalProps) {
  const createBook = useCreateBook();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateBookFormData>({
    resolver: zodResolver(createBookSchema),
    defaultValues: {
      name: "",
      author: "",
      isbn: "",
      year: "",
      actualQuantity: 1,
    },
  });

  const onSubmit = async (data: CreateBookFormData) => {
    try {
      // Převod roku na ISO date string (backend očekává DateTime)
      const yearDate = new Date(parseInt(data.year), 0, 1).toISOString();

      await createBook.mutateAsync({
        name: data.name,
        author: data.author,
        isbn: data.isbn,
        year: yearDate,
        actualQuantity: data.actualQuantity,
      });

      reset();
      onClose();
    } catch (error) {
      console.error("Chyba při vytváření knihy:", error);
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Přidat novou knihu">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input
          id="name"
          label="Název knihy"
          placeholder="např. Harry Potter"
          error={errors.name?.message}
          {...register("name")}
        />

        <Input
          id="author"
          label="Autor"
          placeholder="např. J.K. Rowling"
          error={errors.author?.message}
          {...register("author")}
        />

        <Input
          id="isbn"
          label="ISBN"
          placeholder="např. 9780756419264"
          error={errors.isbn?.message}
          {...register("isbn")}
        />

        <Input
          id="year"
          label="Rok vydání"
          type="number"
          placeholder="např. 2020"
          error={errors.year?.message}
          {...register("year")}
        />

        <Input
          id="actualQuantity"
          label="Počet kusů"
          type="number"
          min={0}
          error={errors.actualQuantity?.message}
          {...register("actualQuantity", { valueAsNumber: true })}
        />

        {createBook.isError && (
          <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-sm text-red-600">
              Chyba při vytváření knihy. Zkontrolujte, zda ISBN již neexistuje.
            </p>
          </div>
        )}

        <div className="flex justify-end gap-3 pt-4">
          <Button type="button" variant="secondary" onClick={handleClose}>
            Zrušit
          </Button>
          <Button
            type="submit"
            isLoading={isSubmitting || createBook.isPending}
          >
            Vytvořit knihu
          </Button>
        </div>
      </form>
    </Modal>
  );
}
