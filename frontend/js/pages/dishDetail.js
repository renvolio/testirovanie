import { api, ApiError } from "../api/client.js";
import { DISH_CATEGORY, DIETARY_FLAG_LABELS } from "../constants/labels.js";
import { escapeHtml, formatDate } from "../utils/html.js";

function formatFlagsNumeric(n) {
  const v = Number(n) || 0;
  if (!v) return "—";
  const parts = [];
  for (const { bit, label } of DIETARY_FLAG_LABELS) {
    if (v & bit) parts.push(label);
  }
  return parts.length ? parts.join(", ") : "—";
}

/**
 * @param {HTMLElement} root
 * @param {string} id
 */
export async function renderDishDetail(root, id) {
  root.innerHTML = `<p class="empty-state"><span class="spinner"></span> Загрузка…</p>`;
  try {
    const d = await api.get(`/api/dishes/${id}`);
    const ing = (d.ingredients || [])
      .map(
        (i) =>
          `<tr><td><a href="#/products/${escapeHtml(i.productId)}">${escapeHtml(i.productName)}</a></td><td>${escapeHtml(String(i.quantityGrams))}</td></tr>`
      )
      .join("");

    root.innerHTML = `
      <h1 class="page-title">${escapeHtml(d.name)}</h1>
      <div class="toolbar">
        <a class="btn btn--ghost" href="#/dishes">← К списку</a>
        <a class="btn btn--secondary" href="#/dishes/${d.id}/edit">Редактировать</a>
        <button type="button" class="btn btn--danger" id="d-del">Удалить</button>
      </div>
      <div class="card">
        <dl class="meta">
          <dt>Категория</dt><dd>${escapeHtml(DISH_CATEGORY[d.category] || d.category)}</dd>
          <dt>Порция</dt><dd>${escapeHtml(String(d.portionSizeGrams))} г</dd>
          <dt>Ккал / порция</dt><dd>${escapeHtml(String(d.caloriesPerPortion))} <span class="badge">расчёт: ${escapeHtml(String(d.suggestedCaloriesPerPortion?.toFixed?.(1) ?? d.suggestedCaloriesPerPortion))}</span></dd>
          <dt>Белки / порция</dt><dd>${escapeHtml(String(d.proteinsPerPortion))} г <span class="badge">расчёт: ${escapeHtml(String(d.suggestedProteinsPerPortion?.toFixed?.(2) ?? d.suggestedProteinsPerPortion))}</span></dd>
          <dt>Жиры / порция</dt><dd>${escapeHtml(String(d.fatsPerPortion))} г</dd>
          <dt>Углеводы / порция</dt><dd>${escapeHtml(String(d.carbsPerPortion))} г</dd>
          <dt>Флаги</dt><dd>${escapeHtml(formatFlagsNumeric(d.additionalFlags))}</dd>
          <dt>Доступные флаги по составу</dt><dd>${escapeHtml(formatFlagsNumeric(d.allowedAdditionalFlags))}</dd>
          <dt>Создано</dt><dd>${escapeHtml(formatDate(d.createdAt))}</dd>
          <dt>Изменено</dt><dd>${escapeHtml(formatDate(d.modifiedAt))}</dd>
        </dl>
        ${(d.photoUrls && d.photoUrls.length)
          ? `<h2 style="margin-top:1rem;font-size:1rem">Фото</h2><ul>${d.photoUrls.map((u) => `<li><a href="${escapeHtml(u)}" target="_blank" rel="noopener">${escapeHtml(u)}</a></li>`).join("")}</ul>`
          : ""}
        <h2 style="margin-top:1.25rem;font-size:1.05rem;color:var(--color-needle)">Состав</h2>
        <div class="table-wrap" style="margin-top:0.5rem">
          <table class="data">
            <thead><tr><th>Продукт</th><th>Г в порции</th></tr></thead>
            <tbody>${ing || `<tr><td colspan="2">—</td></tr>`}</tbody>
          </table>
        </div>
      </div>
    `;

    root.querySelector("#d-del")?.addEventListener("click", async () => {
      if (!confirm("Удалить это блюдо?")) return;
      try {
        await api.delete(`/api/dishes/${id}`);
        window.location.hash = "#/dishes";
      } catch (e) {
        const msg = e instanceof ApiError ? e.message : String(e);
        alert(msg);
      }
    });
  } catch (e) {
    if (e instanceof ApiError && e.status === 404) {
      root.innerHTML = `<div class="alert alert--error">Блюдо не найдено.</div>
        <p><a href="#/dishes" class="btn btn--ghost">К списку</a></p>`;
      return;
    }
    const msg = e instanceof ApiError ? e.message : String(e);
    root.innerHTML = `<div class="alert alert--error">${escapeHtml(msg)}</div>`;
  }
}
