/**
 * Configured Axios instance for API calls.
 *
 * - Base URL from configuration
 * - Automatic Bearer token injection
 * - Token refresh on 401 errors
 * - Request/response logging in debug mode
 */

import axios, { type InternalAxiosRequestConfig, AxiosError } from "axios";
import { config } from "../lib/config";

// =============================================================================
// Axios Instance
// =============================================================================

export const apiClient = axios.create({
  baseURL: config.apiUrl,
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 30000, // 30 seconds
});

// =============================================================================
// Token Management
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

// Flag to prevent multiple simultaneous refresh attempts
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: Error) => void;
}> = [];

const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else if (token) {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

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
  async (error: AxiosError) => {
    const originalRequest = error.config;

    // Debug logging
    if (config.features.debugMode) {
      console.error(
        `❌ API Error: ${originalRequest?.method?.toUpperCase()} ${originalRequest?.url}`,
        {
          status: error.response?.status,
          data: error.response?.data,
          message: error.message,
        },
      );
    }

    // Handle 401 Unauthorized - attempt token refresh
    if (error.response?.status === 401 && originalRequest) {
      // Check if we already tried to refresh for this request
      if (
        (originalRequest as typeof originalRequest & { _retry?: boolean })
          ._retry
      ) {
        return Promise.reject(error);
      }

      if (isRefreshing) {
        // Wait for refresh to complete, then retry
        return new Promise((resolve, reject) => {
          failedQueue.push({
            resolve: (token: string) => {
              if (originalRequest.headers) {
                originalRequest.headers.Authorization = `Bearer ${token}`;
              }
              resolve(apiClient(originalRequest));
            },
            reject: (err: Error) => {
              reject(err);
            },
          });
        });
      }

      (
        originalRequest as typeof originalRequest & { _retry?: boolean }
      )._retry = true;
      isRefreshing = true;

      try {
        // Dynamic import to avoid circular dependency
        const { useAuthStore } = await import("../store/authStore");
        const refreshed = await useAuthStore.getState().refreshToken();

        if (refreshed) {
          const newToken = getAccessToken();
          processQueue(null, newToken);

          // Retry original request with new token
          if (originalRequest.headers && newToken) {
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
          }
          return apiClient(originalRequest);
        } else {
          // Refresh failed - user needs to login again
          processQueue(new Error("Token refresh failed"), null);
          return Promise.reject(error);
        }
      } catch (refreshError) {
        processQueue(refreshError as Error, null);
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  },
);

export default apiClient;
