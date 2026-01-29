import { useState } from "react";
import { Search, X, ChevronDown, ChevronUp } from "lucide-react";
import { Input, Button } from "../ui";
import type { SearchBooksQuery } from "../../types";
import { useTranslation } from "../../store/languageStore";

interface BookSearchProps {
  onSearch: (query: { name?: string; author?: string; isbn?: string }) => void;
  onClear: () => void;
  isSearching: boolean;
  activeQuery: SearchBooksQuery | null;
}

export function BookSearch({
  onSearch,
  onClear,
  isSearching,
  activeQuery,
}: BookSearchProps) {
  const { t } = useTranslation();
  const [isExpanded, setIsExpanded] = useState(false);
  const [name, setName] = useState("");
  const [author, setAuthor] = useState("");
  const [isbn, setIsbn] = useState("");

  const hasAnyValue = name.trim() || author.trim() || isbn.trim();

  const handleSearch = () => {
    if (!hasAnyValue) return;

    onSearch({
      name: name.trim() || undefined,
      author: author.trim() || undefined,
      isbn: isbn.trim() || undefined,
    });
  };

  const handleClear = () => {
    setName("");
    setAuthor("");
    setIsbn("");
    onClear();
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleSearch();
    }
  };

  return (
    <div className="bg-white dark:bg-gray-800 p-4 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 mb-6">
      {/* Hlavní řádek - vždy viditelný */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="flex-1">
          <Input
            placeholder={t.search.placeholder}
            value={name}
            onChange={(e) => setName(e.target.value)}
            onKeyDown={handleKeyDown}
          />
        </div>

        <div className="flex gap-2">
          <Button onClick={handleSearch} disabled={!hasAnyValue}>
            <Search className="h-4 w-4 mr-2" />
            {t.searchBtn}
          </Button>

          {isSearching && (
            <Button variant="secondary" onClick={handleClear}>
              <X className="h-4 w-4 mr-2" />
              {t.clear}
            </Button>
          )}

          <Button variant="ghost" onClick={() => setIsExpanded(!isExpanded)}>
            {isExpanded ? (
              <ChevronUp className="h-4 w-4" />
            ) : (
              <ChevronDown className="h-4 w-4" />
            )}
          </Button>
        </div>
      </div>

      {/* Rozšířené filtry */}
      {isExpanded && (
        <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700 grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Input
            label={t.books.author}
            placeholder={t.search.authorPlaceholder}
            value={author}
            onChange={(e) => setAuthor(e.target.value)}
            onKeyDown={handleKeyDown}
          />
          <Input
            label={t.books.isbn}
            placeholder={t.search.isbnPlaceholder}
            value={isbn}
            onChange={(e) => setIsbn(e.target.value)}
            onKeyDown={handleKeyDown}
          />
        </div>
      )}

      {/* Aktivní filtry */}
      {isSearching && activeQuery && (
        <div className="mt-3 flex flex-wrap gap-2">
          <span className="text-sm text-gray-500 dark:text-gray-400 mr-1">
            {t.search.activeFilters}:
          </span>
          {activeQuery.name && (
            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
              {t.search.name}: {activeQuery.name}
            </span>
          )}
          {activeQuery.author && (
            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200">
              {t.books.author}: {activeQuery.author}
            </span>
          )}
          {activeQuery.isbn && (
            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-purple-100 dark:bg-purple-900 text-purple-800 dark:text-purple-200">
              {t.books.isbn}: {activeQuery.isbn}
            </span>
          )}
        </div>
      )}
    </div>
  );
}
