// UPDATED: reads token from "salon_auth" JSON object
// instead of the old plain "token" string key.
import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7001/api",
});

// Attach JWT to every request
api.interceptors.request.use(config => {
  try {
    const raw = localStorage.getItem("salon_auth");
    if (raw) {
      const auth = JSON.parse(raw);
      if (auth?.token) {
        config.headers.Authorization = `Bearer ${auth.token}`;
      }
    }
  } catch {
    // malformed storage — ignore
  }
  return config;
});

// Handle 401 — session expired
api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      localStorage.removeItem("salon_auth");
      window.location.replace("/login");
    }
    return Promise.reject(error);
  }
);

export default api;