import { API_BASE } from "../config.js";

export class ApiError extends Error {
  /**
   * @param {string} message
   * @param {number} status
   * @param {unknown} body
   */
  constructor(message, status, body) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.body = body;
  }
}

function joinUrl(path) {
  const base = API_BASE.replace(/\/$/, "");
  const p = path.startsWith("/") ? path : `/${path}`;
  return `${base}${p}`;
}

/**
 * @param {string} method
 * @param {string} path
 * @param {{ query?: Record<string, unknown>, body?: unknown }} [opts]
 */
export async function apiJson(method, path, opts = {}) {
  const url = new URL(joinUrl(path));
  if (opts.query) {
    for (const [k, v] of Object.entries(opts.query)) {
      if (v === undefined || v === null || v === "") continue;
      if (typeof v === "boolean") url.searchParams.set(k, v ? "true" : "false");
      else url.searchParams.set(k, String(v));
    }
  }

  /** @type {RequestInit} */
  const init = {
    method,
    headers: { Accept: "application/json" }
  };

  if (opts.body !== undefined) {
    init.headers["Content-Type"] = "application/json";
    init.body = JSON.stringify(opts.body);
  }

  const res = await fetch(url.toString(), init);
  const text = await res.text();
  /** @type {unknown} */
  let json = null;
  if (text) {
    try {
      json = JSON.parse(text);
    } catch {
      json = { raw: text };
    }
  }

  if (!res.ok) {
    const j = json && typeof json === "object" ? json : {};
    const msg =
      j.error ||
      j.title ||
      j.message ||
      (Array.isArray(j.errors) ? j.errors.join("; ") : null) ||
      res.statusText;
    throw new ApiError(String(msg || `HTTP ${res.status}`), res.status, json);
  }

  return json;
}

export const api = {
  get: (path, query) => apiJson("GET", path, { query }),
  post: (path, body) => apiJson("POST", path, { body }),
  put: (path, body) => apiJson("PUT", path, { body }),
  delete: (path) => apiJson("DELETE", path)
};
