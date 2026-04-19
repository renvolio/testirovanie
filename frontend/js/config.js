/**
 * Базовый URL API (без завершающего слэша).
 * При открытии через file:// используется localhost — поднимите бэкенд и при необходимости поменяйте порт.
 */
function detectDefaultApiBase() {
  const { protocol, hostname } = window.location;
  if (protocol === "file:" || !hostname) return "http://localhost:5000";
  if (hostname === "localhost" || hostname === "127.0.0.1") {
    return "http://localhost:5000";
  }
  return `${protocol}//${hostname}:5000`;
}

export const API_BASE = (window.__API_BASE__ || detectDefaultApiBase()).replace(/\/$/, "");
