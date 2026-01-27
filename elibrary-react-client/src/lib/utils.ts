// Formátování data pro zobrazení
export function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString("cs-CZ", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}

// Format yaer z ISO date
export function formatYear(dateString: string): string {
  const date = new Date(dateString);
  return date.getFullYear().toString();
}

// Shortening text
export function truncate(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return text.slice(0, maxLength) + "...";
}

// CSS class name helper
export function cn(...classes: (string | boolean | undefined)[]): string {
  return classes.filter(Boolean).join(" ");
}
