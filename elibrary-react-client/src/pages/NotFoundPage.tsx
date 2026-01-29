import { Link } from "react-router-dom";
import { Home, FileQuestion } from "lucide-react";
import { Button } from "../components";
import { useTranslation } from "../store/languageStore";

export function NotFoundPage() {
  const { t } = useTranslation();

  return (
    <div className="flex flex-col items-center justify-center py-16">
      <FileQuestion className="h-24 w-24 text-gray-400 dark:text-gray-600 mb-6" />

      <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-2">
        404
      </h1>

      <h2 className="text-xl font-semibold text-gray-700 dark:text-gray-300 mb-4">
        {t.notFound.title}
      </h2>

      <p className="text-gray-600 dark:text-gray-400 mb-8 text-center max-w-md">
        {t.notFound.message}
      </p>

      <Link to="/">
        <Button>
          <Home className="h-4 w-4 mr-2" />
          {t.notFound.backHome}
        </Button>
      </Link>
    </div>
  );
}
