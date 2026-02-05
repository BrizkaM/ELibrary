import { useEffect } from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "./lib/queryClient";
import { Layout, ToastContainer } from "./components";
import { HomePage, BooksPage, HistoryPage, NotFoundPage } from "./pages";
import { AuthCallbackPage } from "./pages/AuthCallbackPage";
import { ProtectedRoute } from "./components/auth";
import { useAuthStore, setupTokenRefresh, clearTokenRefresh } from "./store/authStore";

function AppContent() {
  const { checkAuth, isLoading } = useAuthStore();

  // Check authentication status on app load
  useEffect(() => {
    checkAuth();
    setupTokenRefresh();

    return () => {
      clearTokenRefresh();
    };
  }, [checkAuth]);

  // Optional: Show global loading while checking auth
  // if (isLoading) {
  //   return <div>Loading...</div>;
  // }

  return (
    <BrowserRouter>
      <Layout>
        <Routes>
          {/* Public routes */}
          <Route path="/" element={<HomePage />} />
          <Route path="/auth/callback" element={<AuthCallbackPage />} />
          
          {/* Protected routes - require authentication */}
          <Route
            path="/books"
            element={
              <ProtectedRoute>
                <BooksPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/history"
            element={
              <ProtectedRoute>
                <HistoryPage />
              </ProtectedRoute>
            }
          />
          
          {/* 404 */}
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </Layout>
      <ToastContainer />
    </BrowserRouter>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AppContent />
    </QueryClientProvider>
  );
}

export default App;
