import axios, { type InternalAxiosRequestConfig } from "axios";
import { config } from "../lib/config";

/**
 * Configured Axios instance for API calls.
 *
 * Features:
 * - Base URL from configuration
 * - Request/response logging in debug mode
 * - Error handling
 *
 * NOTE: Authentication will be added in Keycloak implementation phase.
 */
export const apiClient = axios.create({
  baseURL: config.apiUrl,
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 30000, // 30 seconds
});

// =============================================================================
// Token Management (placeholder for Keycloak implementation)
// =============================================================================

let accessToken: string | null = null;

/**
 * Sets the access token for API requests.
 * Called by auth store after successful login.
 */
export function setAccessToken(token: string | null): void {
  accessToken = token;
}

/**
 * Gets the current access token.
 */
export function getAccessToken(): string | null {
  return accessToken;
}

/**
 * Clears the access token.
 */
export function clearAccessToken(): void {
  accessToken = null;
}

// =============================================================================
// Request Interceptor
// =============================================================================

apiClient.interceptors.request.use(
  (requestConfig: InternalAxiosRequestConfig) => {
    // Add Authorization header if token exists
    if (accessToken) {
      requestConfig.headers.Authorization = `Bearer ${accessToken}`;
    }

    // Debug logging
    if (config.features.debugMode) {
      console.log(
        `🌐 API Request: ${requestConfig.method?.toUpperCase()} ${requestConfig.url}`,
        {
          headers: requestConfig.headers,
          data: requestConfig.data,
        },
      );
    }

    return requestConfig;
  },
  (error) => {
    if (config.features.debugMode) {
      console.error("🌐 API Request Error:", error);
    }
    return Promise.reject(error);
  },
);

// =============================================================================
// Response Interceptor
// =============================================================================

apiClient.interceptors.response.use(
  (response) => {
    // Debug logging
    if (config.features.debugMode) {
      console.log(
        `✅ API Response: ${response.config.method?.toUpperCase()} ${response.config.url}`,
        {
          status: response.status,
          data: response.data,
        },
      );
    }

    return response;
  },
  async (error) => {
    // Debug logging
    if (config.features.debugMode) {
      console.error(
        `❌ API Error: ${error.config?.method?.toUpperCase()} ${error.config?.url}`,
        {
          status: error.response?.status,
          data: error.response?.data,
          message: error.message,
        },
      );
    }

    // TODO: Handle 401 Unauthorized after Keycloak implementation
    // For now, just reject the error
    if (error.response?.status === 401) {
      console.warn(
        "Unauthorized request - authentication will be implemented with Keycloak",
      );
    }

    return Promise.reject(error);
  },
);

export default apiClient;
