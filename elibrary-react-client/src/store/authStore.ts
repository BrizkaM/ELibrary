/**
 * Authentication Store (Zustand)
 * 
 * Manages authentication state across the application.
 * Integrates with KeycloakService for OAuth 2.0 / OIDC operations.
 */

import { create } from "zustand";
import { keycloakService, type UserInfo } from "../services/keycloakService";
import { setAccessToken, clearAccessToken } from "../api/client";

// =============================================================================
// Types
// =============================================================================

export interface User {
  id: string;
  username: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  roles: string[];
}

interface AuthState {
  // State
  isAuthenticated: boolean;
  isLoading: boolean;
  user: User | null;
  error: string | null;

  // Actions
  login: () => Promise<void>;
  logout: () => void;
  handleCallback: (searchParams: URLSearchParams) => Promise<void>;
  checkAuth: () => Promise<void>;
  refreshToken: () => Promise<boolean>;
  clearError: () => void;

  // Role checks
  hasRole: (role: string) => boolean;
  isAdmin: () => boolean;
  isLibrarian: () => boolean;
}

// =============================================================================
// Helper Functions
// =============================================================================

/**
 * Converts Keycloak token data to our User model
 */
function tokenToUser(): User | null {
  const decoded = keycloakService.decodeToken();
  if (!decoded) return null;

  return {
    id: decoded.sub,
    username: decoded.preferred_username || "unknown",
    email: decoded.email,
    firstName: decoded.given_name,
    lastName: decoded.family_name,
    fullName: decoded.given_name && decoded.family_name
      ? `${decoded.given_name} ${decoded.family_name}`
      : decoded.preferred_username,
    roles: keycloakService.getUserRoles(),
  };
}

// =============================================================================
// Auth Store
// =============================================================================

export const useAuthStore = create<AuthState>((set, get) => ({
  // ---------------------------------------------------------------------------
  // Initial State
  // ---------------------------------------------------------------------------
  isAuthenticated: false,
  isLoading: true, // Start loading to check existing session
  user: null,
  error: null,

  // ---------------------------------------------------------------------------
  // Actions
  // ---------------------------------------------------------------------------

  /**
   * Initiates login flow - redirects to Keycloak
   */
  login: async () => {
    set({ isLoading: true, error: null });
    try {
      await keycloakService.login();
      // Note: This redirects, so we won't reach here
    } catch (error) {
      set({
        isLoading: false,
        error: error instanceof Error ? error.message : "Login failed",
      });
    }
  },

  /**
   * Logs out user - redirects to Keycloak logout
   */
  logout: () => {
    // Clear API client token
    clearAccessToken();

    // Clear store state
    set({
      isAuthenticated: false,
      user: null,
      error: null,
    });

    // Redirect to Keycloak logout
    keycloakService.logout();
  },

  /**
   * Handles OAuth callback after Keycloak login
   */
  handleCallback: async (searchParams: URLSearchParams) => {
    set({ isLoading: true, error: null });

    try {
      // Exchange code for tokens
      const tokens = await keycloakService.handleCallback(searchParams);

      // Set token in API client
      setAccessToken(tokens.access_token);

      // Get user from token
      const user = tokenToUser();

      set({
        isAuthenticated: true,
        user,
        isLoading: false,
        error: null,
      });
    } catch (error) {
      set({
        isAuthenticated: false,
        user: null,
        isLoading: false,
        error: error instanceof Error ? error.message : "Authentication failed",
      });
    }
  },

  /**
   * Checks if user has an existing valid session
   */
  checkAuth: async () => {
    set({ isLoading: true });

    // Check if we have a valid token
    if (keycloakService.isAuthenticated()) {
      const accessToken = keycloakService.getAccessToken();
      if (accessToken) {
        setAccessToken(accessToken);
        const user = tokenToUser();
        set({
          isAuthenticated: true,
          user,
          isLoading: false,
        });
        return;
      }
    }

    // Try to refresh token if access token is expired
    if (keycloakService.isTokenExpired()) {
      const refreshed = await get().refreshToken();
      if (refreshed) {
        set({ isLoading: false });
        return;
      }
    }

    // No valid session
    clearAccessToken();
    set({
      isAuthenticated: false,
      user: null,
      isLoading: false,
    });
  },

  /**
   * Refreshes the access token
   */
  refreshToken: async () => {
    try {
      const tokens = await keycloakService.refreshToken();
      if (tokens) {
        setAccessToken(tokens.access_token);
        const user = tokenToUser();
        set({
          isAuthenticated: true,
          user,
          error: null,
        });
        return true;
      }
    } catch (error) {
      console.error("Token refresh failed", error);
    }

    // Refresh failed
    clearAccessToken();
    set({
      isAuthenticated: false,
      user: null,
    });
    return false;
  },

  /**
   * Clears any auth error
   */
  clearError: () => {
    set({ error: null });
  },

  // ---------------------------------------------------------------------------
  // Role Checks
  // ---------------------------------------------------------------------------

  /**
   * Checks if user has a specific role
   */
  hasRole: (role: string) => {
    const { user } = get();
    return user?.roles.includes(role) ?? false;
  },

  /**
   * Checks if user is an admin
   */
  isAdmin: () => {
    return get().hasRole("admin");
  },

  /**
   * Checks if user is a librarian
   */
  isLibrarian: () => {
    const { hasRole } = get();
    return hasRole("librarian") || hasRole("admin");
  },
}));

// =============================================================================
// Token Refresh Setup
// =============================================================================

/**
 * Sets up automatic token refresh before expiry
 */
let refreshIntervalId: number | null = null;

export function setupTokenRefresh(): void {
  // Clear existing interval
  if (refreshIntervalId) {
    clearInterval(refreshIntervalId);
  }

  // Check and refresh every 4 minutes (tokens expire in 5 min)
  refreshIntervalId = window.setInterval(async () => {
    const store = useAuthStore.getState();
    if (store.isAuthenticated && keycloakService.isTokenExpired()) {
      console.log("🔐 Auto-refreshing token...");
      await store.refreshToken();
    }
  }, 4 * 60 * 1000);
}

export function clearTokenRefresh(): void {
  if (refreshIntervalId) {
    clearInterval(refreshIntervalId);
    refreshIntervalId = null;
  }
}
