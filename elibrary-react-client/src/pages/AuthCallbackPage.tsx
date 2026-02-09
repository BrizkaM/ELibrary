/**
 * Auth Callback Page
 *
 * Handles the OAuth 2.0 callback from Keycloak after successful login.
 * This page receives the authorization code and exchanges it for tokens.
 */

import { useEffect, useRef, useState } from "react";
import { useNavigate, useSearchParams, useLocation } from "react-router-dom";
import { useAuthStore } from "../store/authStore";

export function AuthCallbackPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const location = useLocation();
  const { handleCallback, error, isAuthenticated } = useAuthStore();
  const [localError, setLocalError] = useState<string | null>(null);

  // Guard proti dvojitému zpracování
  const isProcessing = useRef(false);

  useEffect(() => {
    const processCallback = async () => {
      // Prevent double processing (React StrictMode)
      if (isProcessing.current) return;
      isProcessing.current = true;

      try {
        // Check for error in URL params (Keycloak error response)
        const errorParam = searchParams.get("error");
        if (errorParam) {
          const errorDescription =
            searchParams.get("error_description") || errorParam;
          setLocalError(errorDescription);
          return;
        }

        // Process the callback
        await handleCallback(searchParams);
      } catch (err) {
        setLocalError(
          err instanceof Error ? err.message : "Authentication failed",
        );
      }
    };

    processCallback();
  }, [searchParams, handleCallback]);

  // Redirect after successful authentication
  useEffect(() => {
    if (isAuthenticated) {
      // Get the original destination or default to home
      const from =
        (location.state as { from?: { pathname: string } })?.from?.pathname ||
        "/";
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, navigate, location.state]);

  // Show error state
  const displayError = localError || error;
  if (displayError) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50 dark:bg-gray-900">
        <div className="text-center p-8 max-w-md">
          <div className="text-6xl mb-4">❌</div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
            Přihlášení selhalo
          </h1>
          <p className="text-red-600 dark:text-red-400 mb-4">{displayError}</p>
          <button
            onClick={() => navigate("/")}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Zpět na úvodní stránku
          </button>
        </div>
      </div>
    );
  }

  // Loading state while processing callback
  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="text-center p-8">
        <div className="w-12 h-12 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto mb-4" />
        <h1 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
          Přihlašování...
        </h1>
        <p className="text-gray-600 dark:text-gray-400">
          Dokončujeme přihlášení, prosím čekejte.
        </p>
      </div>
    </div>
  );
}
