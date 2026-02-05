/**
 * Protected Route Component
 *
 * Wraps routes that require authentication.
 * Redirects to login if user is not authenticated.
 * Optionally checks for specific roles.
 */

/**
 * Protected Route Component
 *
 * Wraps routes that require authentication.
 * Redirects to login if user is not authenticated.
 * Optionally checks for specific roles.
 */
import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuthStore } from "../../store/authStore";

interface ProtectedRouteProps {
  children: ReactNode;
  /** Required roles (user must have at least one) */
  roles?: string[];
  /** If true, user must have ALL specified roles */
  requireAllRoles?: boolean;
  /** Custom redirect path for unauthorized users */
  redirectTo?: string;
}

export function ProtectedRoute({
  children,
  roles,
  requireAllRoles = false,
  redirectTo = "/",
}: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, user, hasRole } = useAuthStore();
  const location = useLocation();

  // Show loading while checking auth
  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="flex flex-col items-center gap-4">
          <div className="w-8 h-8 border-4 border-blue-500 border-t-transparent rounded-full animate-spin" />
          <p className="text-gray-600 dark:text-gray-400">Ověřování...</p>
        </div>
      </div>
    );
  }

  // Not authenticated - redirect to home (or login)
  if (!isAuthenticated) {
    // Store the attempted URL for redirect after login
    return <Navigate to={redirectTo} state={{ from: location }} replace />;
  }

  // Check role requirements
  if (roles && roles.length > 0) {
    const hasRequiredRole = requireAllRoles
      ? roles.every((role) => hasRole(role))
      : roles.some((role) => hasRole(role));

    if (!hasRequiredRole) {
      // User authenticated but doesn't have required role
      return (
        <div className="flex items-center justify-center min-h-screen">
          <div className="text-center p-8 max-w-md">
            <div className="text-6xl mb-4">🔒</div>
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
              Přístup odepřen
            </h1>
            <p className="text-gray-600 dark:text-gray-400 mb-4">
              Nemáte oprávnění pro přístup k této stránce.
            </p>
            <p className="text-sm text-gray-500 dark:text-gray-500">
              Požadované role: {roles.join(", ")}
            </p>
            <p className="text-sm text-gray-500 dark:text-gray-500 mt-1">
              Vaše role: {user?.roles.join(", ") || "žádné"}
            </p>
          </div>
        </div>
      );
    }
  }

  // All checks passed - render children
  return <>{children}</>;
}

/**
 * Admin-only route shorthand
 */
export function AdminRoute({ children }: { children: ReactNode }) {
  return <ProtectedRoute roles={["admin"]}>{children}</ProtectedRoute>;
}

/**
 * Librarian route shorthand (includes admin)
 */
export function LibrarianRoute({ children }: { children: ReactNode }) {
  return (
    <ProtectedRoute roles={["librarian", "admin"]}>{children}</ProtectedRoute>
  );
}
