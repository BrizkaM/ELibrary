import { create } from "zustand";
import { persist } from "zustand/middleware";
import { translations, type Language } from "../lib/i18n/translations";

interface LanguageStore {
  language: Language;
  setLanguage: (language: Language) => void;
}

export const useLanguageStore = create<LanguageStore>()(
  persist(
    (set) => ({
      language: "cs",

      setLanguage: (language) => {
        set({ language });
      },
    }),
    {
      name: "elibrary-language",
    },
  ),
);

// Helper hook pro snadné použití
export const useTranslation = () => {
  const { language, setLanguage } = useLanguageStore();
  const t = translations[language];

  // Funkce pro interpolaci {placeholder}
  const translate = (
    text: string,
    params?: Record<string, string | number>,
  ) => {
    if (!params) return text;

    return Object.entries(params).reduce(
      (acc, [key, value]) => acc.replace(`{${key}}`, String(value)),
      text,
    );
  };

  return { t, language, setLanguage, translate };
};
