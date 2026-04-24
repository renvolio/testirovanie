import { escapeHtml } from "./html.js";

export function shortenUrlText(url, maxLen = 48) {
  const s = String(url || "");
  if (s.length <= maxLen) return s;
  return s.slice(0, Math.max(0, maxLen - 1)) + "…";
}

export function linkHtml(url, maxLen = 48) {
  const u = String(url || "").trim();
  if (!u) return "";
  const text = escapeHtml(shortenUrlText(u, maxLen));
  const href = escapeHtml(u);
  return `<a class="truncate" href="${href}" target="_blank" rel="noopener">${text}</a>`;
}

