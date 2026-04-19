import { api, ApiError } from "../api/client.js";
import { escapeHtml } from "../utils/html.js";

export async function renderHome(root) {
  root.innerHTML = `<p class="empty-state"><span class="spinner" aria-hidden="true"></span> Проверка API…</p>`;
  const foot = document.getElementById("footer-api-status");
  try {
    const h = await api.get("/api/health");
    foot.textContent = `API: ${h.status || "ok"}`;
    root.innerHTML = `
      <h1 class="page-title">Главная</h1>
      <div class="card">
        <p>Сервер отвечает: <strong>${escapeHtml(String(h.status || "ok"))}</strong></p>
        <p class="hint" style="color:var(--color-muted);margin-top:0.75rem">
          Разделы: <a href="#/products">продукты</a>, <a href="#/dishes">блюда</a>.
        </p>
      </div>
    `;
  } catch (e) {
    foot.textContent = "API: недоступен";
    const msg = e instanceof ApiError ? e.message : String(e);
    root.innerHTML = `
      <h1 class="page-title">Главная</h1>
      <div class="alert alert--error">
        Не удалось связаться с API (${escapeHtml(msg)}). Убедитесь, что бэкенд запущен и CORS настроен.
      </div>
    `;
  }
}
