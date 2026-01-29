// Formátování data pro zobrazení
export function formatDate(dateString: string, locale: string = "cs"): string {
  const date = new Date(dateString);
  return date.toLocaleDateString(locale === "cs" ? "cs-CZ" : "en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}

// Formátování času
export function formatTime(dateString: string, locale: string = "cs"): string {
  const date = new Date(dateString);
  return date.toLocaleTimeString(locale === "cs" ? "cs-CZ" : "en-US", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

// Formátování data a času
export function formatDateTime(
  dateString: string,
  locale: string = "cs",
): string {
  return `${formatDate(dateString, locale)}, ${formatTime(dateString, locale)}`;
}

// Formátování roku z ISO date
export function formatYear(dateString: string): string {
  const date = new Date(dateString);
  return date.getFullYear().toString();
}

// Zkrácení textu
export function truncate(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return text.slice(0, maxLength) + "...";
}

// CSS class name helper (jednoduchá verze cn)
export function cn(...classes: (string | boolean | undefined)[]): string {
  return classes.filter(Boolean).join(" ");
}
