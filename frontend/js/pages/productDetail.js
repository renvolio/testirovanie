import { api, ApiError } from "../api/client.js";
import {
  PRODUCT_CATEGORY,
  COOKING,
  formatDietaryFlags
} from "../constants/labels.js";
import { escapeHtml, formatDate } from "../utils/html.js";
import { openModal, closeModal } from "../ui/modal.js";
import { linkHtml } from "../utils/urls.js";

/**
 * @param {HTMLElement} root
 * @param {string} id
 */
export async function renderProductDetail(root, id) {
  root.innerHTML = `<p class="empty-state"><span class="spinner"></span> Загрузка…</p>`;
  try {
    const p = await api.get(`/api/products/${id}`);
    root.innerHTML = `
      <h1 class="page-title">${escapeHtml(p.name)}</h1>
      <div class="toolbar">
        <a class="btn btn--ghost" href="#/products">← К списку</a>
        <a class="btn btn--secondary" href="#/products/${p.id}/edit">Редактировать</a>
        <button type="button" class="btn btn--danger" id="p-del">Удалить</button>
      </div>
      <div class="card">
        <dl class="meta">
          <dt>Категория</dt><dd>${escapeHtml(PRODUCT_CATEGORY[p.category] || p.category)}</dd>
          <dt>Готовность</dt><dd>${escapeHtml(COOKING[p.cookingRequirement] || p.cookingRequirement)}</dd>
          <dt>Ккал / 100 г</dt><dd>${escapeHtml(String(p.caloriesPer100g))}</dd>
          <dt>Белки / 100 г</dt><dd>${escapeHtml(String(p.proteinsPer100g))}</dd>
          <dt>Жиры / 100 г</dt><dd>${escapeHtml(String(p.fatsPer100g))}</dd>
          <dt>Углеводы / 100 г</dt><dd>${escapeHtml(String(p.carbsPer100g))}</dd>
          <dt>Флаги</dt><dd>${escapeHtml(formatDietaryFlags(p.additionalFlags))}</dd>
          <dt>Создан</dt><dd>${escapeHtml(formatDate(p.createdAt))}</dd>
          <dt>Изменён</dt><dd>${escapeHtml(formatDate(p.modifiedAt))}</dd>
        </dl>
        ${p.composition ? `<h2 style="margin-top:1rem;font-size:1rem;color:var(--color-needle)">Состав</h2><p style="white-space:pre-wrap">${escapeHtml(p.composition)}</p>` : ""}
        ${(p.photoUrls && p.photoUrls.length)
          ? `<h2 style="margin-top:1rem;font-size:1rem">Фото</h2>
             <div class="stack" style="gap:0.5rem">
               ${p.photoUrls
                 .map(
                   (u) => `
                 <div>
                   <div class="hint">${linkHtml(u, 60)}</div>
                   <img src="${escapeHtml(u)}" alt="" style="max-width:100%;max-height:260px;border-radius:var(--radius-sm);border:1px solid rgba(20,51,42,0.15)" />
                 </div>
               `
                 )
                 .join("")}
             </div>`
          : ""}
      </div>
    `;

    root.querySelector("#p-del")?.addEventListener("click", async () => {
      if (!confirm("Удалить этот продукт?")) return;
      try {
        await api.delete(`/api/products/${id}`);
        window.location.hash = "#/products";
      } catch (e) {
        if (e instanceof ApiError && e.status === 409) {
          const b = e.body;
          const dishes = Array.isArray(b?.dishes) ? b.dishes : [];
          const list = dishes
            .map(
              (d) =>
                `<li><a href="#/dishes/${escapeHtml(d.id)}">${escapeHtml(d.name)}</a></li>`
            )
            .join("");
          openModal({
            title: "Удаление невозможно",
            bodyHtml: `
              <p>${escapeHtml(b?.message || "Продукт используется в блюдах.")}</p>
              ${list ? `<p><strong>Блюда:</strong></p><ul>${list}</ul>` : ""}
            `,
            actionsHtml: `<div class="modal-actions"><button type="button" class="btn btn--secondary js-close">Закрыть</button></div>`
          });
          return;
        }
        const msg = e instanceof ApiError ? e.message : String(e);
        alert(msg);
      }
    });
  } catch (e) {
    if (e instanceof ApiError && e.status === 404) {
      root.innerHTML = `<div class="alert alert--error">Продукт не найден.</div>
        <p><a href="#/products" class="btn btn--ghost">К списку</a></p>`;
      return;
    }
    const msg = e instanceof ApiError ? e.message : String(e);
    root.innerHTML = `<div class="alert alert--error">${escapeHtml(msg)}</div>`;
  }
}
