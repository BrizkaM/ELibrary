import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMemo } from "react";
import { Button, Input, Modal } from "../ui";
import { useCreateBook } from "../../hooks";
import { useToast } from "../../store/toastStore";
import { useTranslation } from "../../store/languageStore";
import {
  createBookSchema,
  type CreateBookFormData,
} from "../../lib/validations";
import { getErrorMessage } from "../../lib/errorHandler";

interface CreateBookModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CreateBookModal({ isOpen, onClose }: CreateBookModalProps) {
  const createBook = useCreateBook();
  const toast = useToast();
  const { t, translate } = useTranslation();

  const schema = useMemo(() => createBookSchema(t), [t]);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateBookFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: "",
      author: "",
      isbn: "",
      year: "",
      actualQuantity: "1",
    },
  });

  const onSubmit = async (data: CreateBookFormData) => {
    try {
      const yearDate = new Date(parseInt(data.year), 0, 1).toISOString();

      await createBook.mutateAsync({
        name: data.name,
        author: data.author,
        isbn: data.isbn,
        year: yearDate,
        actualQuantity: parseInt(data.actualQuantity),
      });

      toast.success(translate(t.createBook.success, { name: data.name }));
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
    <Modal isOpen={isOpen} onClose={handleClose} title={t.createBook.title}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input
          id="name"
          label={t.createBook.nameLabel}
          placeholder={t.createBook.namePlaceholder}
          error={errors.name?.message}
          {...register("name")}
        />

        <Input
          id="author"
          label={t.createBook.authorLabel}
          placeholder={t.createBook.authorPlaceholder}
          error={errors.author?.message}
          {...register("author")}
        />

        <Input
          id="isbn"
          label={t.createBook.isbnLabel}
          placeholder={t.createBook.isbnPlaceholder}
          error={errors.isbn?.message}
          {...register("isbn")}
        />

        <Input
          id="year"
          label={t.createBook.yearLabel}
          type="number"
          placeholder={t.createBook.yearPlaceholder}
          error={errors.year?.message}
          {...register("year")}
        />

        <Input
          id="actualQuantity"
          label={t.createBook.quantityLabel}
          type="number"
          min={0}
          error={errors.actualQuantity?.message}
          {...register("actualQuantity")}
        />

        <div className="flex justify-end gap-3 pt-4">
          <Button type="button" variant="secondary" onClick={handleClose}>
            {t.cancel}
          </Button>
          <Button
            type="submit"
            isLoading={isSubmitting || createBook.isPending}
          >
            {t.createBook.submit}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
