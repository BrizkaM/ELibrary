import axios from "axios";

// base for API calls
export const apiClient = axios.create({
  baseURL: "https://localhost:7001/api/v1",
  headers: {
    "Content-Type": "application/json",
  },
});

// Interceptor for logging
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error("API Error:", error.response?.data || error.message);
    return Promise.reject(error);
  },
);
