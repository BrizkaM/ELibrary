import { Languages } from "lucide-react";
import { useLanguageStore } from "../../store/languageStore";
import { Button } from "./Button";

export function LanguageToggle() {
  const { language, setLanguage } = useLanguageStore();

  const toggleLanguage = () => {
    setLanguage(language === "cs" ? "en" : "cs");
  };

  return (
    <Button
      variant="ghost"
      size="sm"
      onClick={toggleLanguage}
      title={language === "cs" ? "Switch to English" : "Přepnout do češtiny"}
    >
      <Languages className="h-5 w-5 mr-1" />
      <span className="text-xs font-medium uppercase">{language}</span>
    </Button>
  );
}
