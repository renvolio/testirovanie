import { api, ApiError } from "../api/client.js";
import {
  PRODUCT_CATEGORY,
  COOKING,
  DIETARY_FLAG_LABELS
} from "../constants/labels.js";
import { escapeHtml } from "../utils/html.js";

function categoryOptions() {
  return Object.entries(PRODUCT_CATEGORY)
    .map(([v, l]) => `<option value="${escapeHtml(v)}">${escapeHtml(l)}</option>`)
    .join("");
}

function cookingOptions() {
  return Object.entries(COOKING)
    .map(([v, l]) => `<option value="${escapeHtml(v)}">${escapeHtml(l)}</option>`)
    .join("");
}

function parsePhotos(text) {
  return text
    .split(/\r?\n/)
    .map((s) => s.trim())
    .filter(Boolean)
    .slice(0, 5);
}

function flagsFromCheckboxes(root) {
  let m = 0;
  for (const { bit, key } of DIETARY_FLAG_LABELS) {
    if (root.querySelector(`input[name="flag_${key}"]`)?.checked) m |= bit;
  }
  return m;
}

function setFlagsCheckboxes(root, flags) {
  const n = Number(flags) || 0;
  for (const { bit, key } of DIETARY_FLAG_LABELS) {
    const el = root.querySelector(`input[name="flag_${key}"]`);
    if (el) el.checked = !!(n & bit);
  }
}

/**
 * @param {HTMLElement} root
 * @param {string | null} id
 */
export async function renderProductForm(root, id) {
  const isEdit = Boolean(id);
  root.innerHTML = `<p class="empty-state"><span class="spinner"></span> Загрузка…</p>`;

  /** @type {Record<string, unknown> | null} */
  let existing = null;
  if (isEdit) {
    try {
      existing = await api.get(`/api/products/${id}`);
    } catch (e) {
      const msg = e instanceof ApiError ? e.message : String(e);
      root.innerHTML = `<div class="alert alert--error">${escapeHtml(msg)}</div>
        <p><a href="#/products" class="btn btn--ghost">К списку</a></p>`;
      return;
    }
  }

  root.innerHTML = `
    <h1 class="page-title">${isEdit ? "Редактирование продукта" : "Новый продукт"}</h1>
    <p><a href="#/products" class="btn btn--ghost btn--sm">← К списку</a></p>
    <div id="pform-err"></div>
    <form class="card form-grid form-grid--2" id="pform" style="margin-top:1rem">
      <div class="field" style="grid-column:1/-1">
        <label for="p-name">Название *</label>
        <input type="text" id="p-name" name="productName" required minlength="2" autocomplete="off" />
      </div>
      <div class="field" style="grid-column:1/-1">
        <label for="p-photos">Ссылки на фото (до 5, по одной в строке)</label>
        <textarea id="p-photos" name="photos" rows="3" placeholder="https://…"></textarea>
      </div>
      <div class="field">
        <label for="p-cal">Ккал на 100 г *</label>
        <input type="number" id="p-cal" name="caloriesPer100g" step="any" min="0" required />
      </div>
      <div class="field">
        <label for="p-prot">Белки на 100 г *</label>
        <input type="number" id="p-prot" name="proteinsPer100g" step="any" min="0" max="100" required />
      </div>
      <div class="field">
        <label for="p-fat">Жиры на 100 г *</label>
        <input type="number" id="p-fat" name="fatsPer100g" step="any" min="0" max="100" required />
      </div>
      <div class="field">
        <label for="p-carb">Углеводы на 100 г *</label>
        <input type="number" id="p-carb" name="carbsPer100g" step="any" min="0" max="100" required />
      </div>
      <div class="field" style="grid-column:1/-1">
        <p class="hint" id="p-bju-sum" aria-live="polite"></p>
      </div>
      <div class="field" style="grid-column:1/-1">
        <label for="p-comp">Состав (текст)</label>
        <textarea id="p-comp" name="composition" rows="4"></textarea>
      </div>
      <div class="field">
        <label for="p-cat">Категория *</label>
        <select id="p-cat" name="category" required>${categoryOptions()}</select>
      </div>
      <div class="field">
        <label for="p-cook">Необходимость готовки *</label>
        <select id="p-cook" name="cookingRequirement" required>${cookingOptions()}</select>
      </div>
      <div class="field" style="grid-column:1/-1">
        <span class="hint">Дополнительные флаги</span>
        <div class="stack" style="margin-top:0.35rem">
          ${DIETARY_FLAG_LABELS.map(
            (f) => `
            <label><input type="checkbox" name="flag_${f.key}" /> ${escapeHtml(f.label)}</label>
          `
          ).join("")}
        </div>
      </div>
      <div class="field" style="grid-column:1/-1">
        <button type="submit" class="btn btn--primary">${isEdit ? "Сохранить" : "Создать"}</button>
      </div>
    </form>
  `;

  const form = root.querySelector("#pform");
  const errEl = root.querySelector("#pform-err");
  const sumEl = root.querySelector("#p-bju-sum");

  function updateBjuHint() {
    const pr = Number(form.proteinsPer100g.value);
    const ft = Number(form.fatsPer100g.value);
    const cr = Number(form.carbsPer100g.value);
    if ([pr, ft, cr].some((x) => Number.isNaN(x))) {
      sumEl.textContent = "";
      return;
    }
    const s = pr + ft + cr;
    sumEl.textContent = `Сумма БЖУ: ${s.toFixed(1)} г / 100 г (максимум 100).`;
    sumEl.style.color = s > 100 ? "var(--color-danger)" : "var(--color-muted)";
  }

  ["proteinsPer100g", "fatsPer100g", "carbsPer100g"].forEach((name) => {
    form.elements.namedItem(name)?.addEventListener("input", updateBjuHint);
  });

  if (existing) {
    form.querySelector("#p-name").value = existing.name || "";
    form.querySelector("#p-photos").value = (existing.photoUrls || []).join("\n");
    form.caloriesPer100g.value = String(existing.caloriesPer100g ?? "");
    form.proteinsPer100g.value = String(existing.proteinsPer100g ?? "");
    form.fatsPer100g.value = String(existing.fatsPer100g ?? "");
    form.carbsPer100g.value = String(existing.carbsPer100g ?? "");
    form.composition.value = existing.composition || "";
    form.category.value = existing.category;
    form.cookingRequirement.value = existing.cookingRequirement;
    setFlagsCheckboxes(root, existing.additionalFlags);
  }
  updateBjuHint();

  form.addEventListener("submit", async (ev) => {
    ev.preventDefault();
    errEl.innerHTML = "";
    const pr = Number(form.proteinsPer100g.value);
    const ft = Number(form.fatsPer100g.value);
    const cr = Number(form.carbsPer100g.value);
    if (pr + ft + cr > 100.000001) {
      errEl.innerHTML = `<div class="alert alert--error">Сумма белков, жиров и углеводов на 100 г не может превышать 100 г.</div>`;
      return;
    }

    const body = {
      name: form.querySelector("#p-name").value.trim(),
      photoUrls: parsePhotos(form.querySelector("#p-photos").value),
      caloriesPer100g: Number(form.caloriesPer100g.value),
      proteinsPer100g: pr,
      fatsPer100g: ft,
      carbsPer100g: cr,
      composition: form.composition.value.trim() || null,
      category: form.category.value,
      cookingRequirement: form.cookingRequirement.value,
      additionalFlags: flagsFromCheckboxes(root)
    };

    try {
      if (isEdit) {
        await api.put(`/api/products/${id}`, body);
        window.location.hash = `#/products/${id}`;
      } else {
        const created = await api.post("/api/products", body);
        window.location.hash = `#/products/${created.id}`;
      }
    } catch (e) {
      const msg = e instanceof ApiError ? e.message : String(e);
      errEl.innerHTML = `<div class="alert alert--error">${escapeHtml(msg)}</div>`;
    }
  });
}
