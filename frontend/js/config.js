/**
 * Базовый URL API (без завершающего слэша).
 * При открытии через file:// используется localhost — поднимите бэкенд и при необходимости поменяйте порт.
 */
function detectDefaultApiBase() {
  const { protocol, hostname } = window.location;
  if (protocol === "file:" || !hostname) return "http://localhost:5051";
  if (hostname === "localhost" || hostname === "127.0.0.1") {
    return "http://localhost:5051";
  }
  return `${protocol}//${hostname}:5051`;
}

export const API_BASE = (window.__API_BASE__ || detectDefaultApiBase()).replace(/\/$/, "");
