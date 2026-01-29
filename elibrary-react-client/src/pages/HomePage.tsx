import { Link } from "react-router-dom";
import { BookOpen, History, Plus } from "lucide-react";
import { Card, CardContent } from "../components";
import { useTranslation } from "../store/languageStore";

export function HomePage() {
  const { t } = useTranslation();

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          {t.home.title}
        </h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          {t.home.subtitle}
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Link to="/books">
          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <CardContent className="flex flex-col items-center py-8">
              <BookOpen className="h-12 w-12 text-blue-600 mb-4" />
              <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
                {t.home.booksCard}
              </h2>
              <p className="mt-2 text-gray-600 dark:text-gray-400 text-center">
                {t.home.booksCardDesc}
              </p>
            </CardContent>
          </Card>
        </Link>

        <Link to="/books?action=create">
          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <CardContent className="flex flex-col items-center py-8">
              <Plus className="h-12 w-12 text-green-600 mb-4" />
              <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
                {t.home.addBookCard}
              </h2>
              <p className="mt-2 text-gray-600 dark:text-gray-400 text-center">
                {t.home.addBookCardDesc}
              </p>
            </CardContent>
          </Card>
        </Link>

        <Link to="/history">
          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <CardContent className="flex flex-col items-center py-8">
              <History className="h-12 w-12 text-purple-600 mb-4" />
              <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
                {t.home.historyCard}
              </h2>
              <p className="mt-2 text-gray-600 dark:text-gray-400 text-center">
                {t.home.historyCardDesc}
              </p>
            </CardContent>
          </Card>
        </Link>
      </div>
    </div>
  );
}
