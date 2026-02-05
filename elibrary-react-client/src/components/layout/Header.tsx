import { useState } from "react";
import { BookOpen, History, Home, Menu, X } from "lucide-react";
import { Link, useLocation } from "react-router-dom";
import { cn } from "../../lib/utils";
import { LanguageToggle, ThemeToggle } from "../ui";
import { UserMenu } from "../auth";
import { useTranslation } from "../../store/languageStore";
import { useAuthStore } from "../../store/authStore";

export function Header() {
  const location = useLocation();
  const { t } = useTranslation();
  const { isAuthenticated, login } = useAuthStore();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  // Navigation items - some may require authentication
  const navigation = [
    { name: t.nav.home, href: "/", icon: Home, requiresAuth: false },
    { name: t.nav.books, href: "/books", icon: BookOpen, requiresAuth: true },
    { name: t.nav.history, href: "/history", icon: History, requiresAuth: true },
  ];

  // Filter navigation based on auth status (optional - show all, let ProtectedRoute handle it)
  const visibleNavigation = navigation.filter(
    (item) => !item.requiresAuth || isAuthenticated
  );

  const closeMobileMenu = () => setIsMobileMenuOpen(false);

  return (
    <header className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <Link
            to="/"
            className="flex items-center gap-2"
            onClick={closeMobileMenu}
          >
            <BookOpen className="h-8 w-8 text-blue-600" />
            <span className="text-xl font-bold text-gray-900 dark:text-white">
              {t.appName}
            </span>
          </Link>

          {/* Desktop Navigation */}
          <nav className="hidden md:flex items-center gap-1">
            {visibleNavigation.map((item) => {
              const isActive = location.pathname === item.href;
              return (
                <Link
                  key={item.href}
                  to={item.href}
                  className={cn(
                    "flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors",
                    isActive
                      ? "bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300"
                      : "text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-white",
                  )}
                >
                  <item.icon className="h-4 w-4" />
                  {item.name}
                </Link>
              );
            })}

            {/* Divider */}
            <div className="ml-2 border-l border-gray-200 dark:border-gray-700 pl-2 flex items-center gap-1">
              <LanguageToggle />
              <ThemeToggle />
            </div>

            {/* User Menu */}
            <div className="ml-2 border-l border-gray-200 dark:border-gray-700 pl-3">
              <UserMenu />
            </div>
          </nav>

          {/* Mobile: Controls */}
          <div className="flex items-center gap-2 md:hidden">
            <LanguageToggle />
            <ThemeToggle />
            <UserMenu />
            <button
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
              className="p-2 rounded-lg text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700"
            >
              {isMobileMenuOpen ? (
                <X className="h-6 w-6" />
              ) : (
                <Menu className="h-6 w-6" />
              )}
            </button>
          </div>
        </div>
      </div>

      {/* Mobile Navigation */}
      {isMobileMenuOpen && (
        <div className="md:hidden border-t border-gray-200 dark:border-gray-700">
          <nav className="px-4 py-3 space-y-1">
            {visibleNavigation.map((item) => {
              const isActive = location.pathname === item.href;
              return (
                <Link
                  key={item.href}
                  to={item.href}
                  onClick={closeMobileMenu}
                  className={cn(
                    "flex items-center gap-3 px-4 py-3 rounded-lg text-base font-medium transition-colors",
                    isActive
                      ? "bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300"
                      : "text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-white",
                  )}
                >
                  <item.icon className="h-5 w-5" />
                  {item.name}
                </Link>
              );
            })}
            
            {/* Mobile: Login prompt for protected items when not authenticated */}
            {!isAuthenticated && navigation.some((item) => item.requiresAuth) && (
              <div className="pt-2 border-t border-gray-200 dark:border-gray-700 mt-2">
                <button
                  onClick={() => {
                    closeMobileMenu();
                    login();
                  }}
                  className="flex items-center gap-3 px-4 py-3 rounded-lg text-base font-medium text-blue-600 dark:text-blue-400 hover:bg-gray-100 dark:hover:bg-gray-700 w-full"
                >
                  Přihlaste se pro více funkcí
                </button>
              </div>
            )}
          </nav>
        </div>
      )}
    </header>
  );
}
