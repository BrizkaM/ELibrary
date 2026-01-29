import { Moon, Sun } from "lucide-react";
import { useThemeStore } from "../../store/themeStore";
import { Button } from "./Button";

export function ThemeToggle() {
  const { theme, toggleTheme } = useThemeStore();

  return (
    <Button
      variant="ghost"
      size="sm"
      onClick={toggleTheme}
      title={
        theme === "light"
          ? "Přepnout na tmavý režim"
          : "Přepnout na světlý režim"
      }
    >
      {theme === "light" ? (
        <Moon className="h-5 w-5" />
      ) : (
        <Sun className="h-5 w-5" />
      )}
    </Button>
  );
}
