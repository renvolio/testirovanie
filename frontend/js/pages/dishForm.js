import { api, ApiError } from "../api/client.js";
import { DISH_CATEGORY, DIETARY_FLAG_LABELS, parseDietaryFlags } from "../constants/labels.js";
import { escapeHtml } from "../utils/html.js";

function dishCategoryOptions() {
  return `<option value="">— из макроса в названии (!суп, !десерт, …) —</option>
  ${Object.entries(DISH_CATEGORY)
    .map(([v, l]) => `<option value="${escapeHtml(v)}">${escapeHtml(l)}</option>`)
    .join("")}`;
}

function parsePhotos(text) {
  return text
    .split(/\r?\n/)
    .map((s) => s.trim())
    .filter(Boolean)
    .slice(0, 5);
}

/** @param {Map<string, object>} productById */
function allowedFlagsFromProducts(productById, ids) {
  if (!ids.length) return 0;
  let bits = 0;
  let okV = true,
    okG = true,
    okS = true;
  for (const id of ids) {
    const p = productById.get(id);
    if (!p) {
      okV = okG = okS = false;
      break;
    }
    const f = parseDietaryFlags(p.additionalFlags);
    if ((f & 1) !== 1) okV = false;
    if ((f & 2) !== 2) okG = false;
    if ((f & 4) !== 4) okS = false;
  }
  if (okV) bits |= 1;
  if (okG) bits |= 2;
  if (okS) bits |= 4;
  return bits;
}

function flagsFromCheckboxes(root) {
  let m = 0;
  for (const { bit, key } of DIETARY_FLAG_LABELS) {
    const el = root.querySelector(`input[name="dflag_${key}"]`);
    if (el?.checked && !el.disabled) m |= bit;
  }
  return m;
}

function setDishFlags(root, flags, allowed) {
  const n = parseDietaryFlags(flags);
  const a = parseDietaryFlags(allowed);
  for (const { bit, key } of DIETARY_FLAG_LABELS) {
    const el = root.querySelector(`input[name="dflag_${key}"]`);
    if (!el) continue;
    const can = (a & bit) === bit;
    el.disabled = !can;
    el.checked = can && (n & bit) === bit;
  }
}

function compositionRowHtml(productOptions) {
  return `
    <div class="composition-row" data-row>
      <div class="field" style="margin:0">
        <label class="hint">Продукт</label>
        <select class="js-prod" required>
          <option value="">— выберите —</option>
          ${productOptions}
        </select>
      </div>
      <div class="field" style="margin:0">
        <label class="hint">грамм</label>
        <input type="number" class="js-qty" step="any" min="0.0001" required />
      </div>
      <button type="button" class="btn btn--ghost btn--sm js-remove">Удалить</button>
    </div>
  `;
}

/**
 * @param {HTMLElement} root
 * @param {string | null} id
 */
export async function renderDishForm(root, id) {
  const isEdit = Boolean(id);
  root.innerHTML = `<p class="empty-state"><span class="spinner"></span> Загрузка…</p>`;

  let products = [];
  try {
    products = await api.get("/api/products", { sortBy: "name", sortDescending: false });
  } catch {
    root.innerHTML = `<div class="alert alert--error">Не удалось загрузить список продуктов.</div>`;
    return;
  }

  const productById = new Map(products.map((p) => [p.id, p]));
  const productOptions = products
    .map((p) => `<option value="${escapeHtml(p.id)}">${escapeHtml(p.name)}</option>`)
    .join("");

  /** @type {Record<string, unknown> | null} */
  let existing = null;
  if (isEdit) {
    try {
      existing = await api.get(`/api/dishes/${id}`);
    } catch (e) {
      const msg = e instanceof ApiError ? e.message : String(e);
      root.innerHTML = `<div class="alert alert--error">${escapeHtml(msg)}</div>
        <p><a href="#/dishes" class="btn btn--ghost">К списку</a></p>`;
      return;
    }
  }

  root.innerHTML = `
    <h1 class="page-title">${isEdit ? "Редактирование блюда" : "Новое блюдо"}</h1>
    <p><a href="#/dishes" class="btn btn--ghost btn--sm">← К списку</a></p>
    <div id="dform-err"></div>
    <form class="card form-grid" id="dform" style="margin-top:1rem">
      <div class="field" style="grid-column:1/-1">
        <label for="d-name">Название *</label>
        <input type="text" id="d-name" name="dishName" required autocomplete="off" />
        <p class="hint">Можно указать категорию макросом: !десерт, !первое, !второе, !напиток, !салат, !суп, !перекус (без учёта регистра). Если задана категория ниже — она важнее макроса.</p>
      </div>
      <div class="field" style="grid-column:1/-1">
        <label for="d-photos">Ссылки на фото (до 5, по строке)</label>
        <textarea id="d-photos" name="dishPhotos" rows="2"></textarea>
      </div>
      <div class="field">
        <label for="d-portion">Размер порции, г *</label>
        <input type="number" id="d-portion" name="dishPortion" step="any" min="0.0001" required />
      </div>
      <div class="field">
        <label for="d-cat">Категория</label>
        <select id="d-cat" name="dishCategory">${dishCategoryOptions()}</select>
      </div>
      <div class="field" style="grid-column:1/-1">
        <label>Состав *</label>
        <div id="d-comp-rows"></div>
        <button type="button" class="btn btn--ghost btn--sm" id="d-add-row" style="margin-top:0.5rem">+ Продукт</button>
      </div>
      <fieldset style="border:1px solid rgba(20,51,42,0.15);border-radius:var(--radius-sm);padding:0.75rem;margin:0">
        <legend style="font-weight:600;color:var(--color-needle)">КБЖУ на порцию</legend>
        <p class="hint">Оставьте поле пустым — подставится расчёт по составу после сохранения. Можно ввести своё значение.</p>
        <p class="hint" id="d-calc-now" style="color:var(--color-muted)"></p>
        <div class="form-grid form-grid--2" style="margin-top:0.5rem">
          <div class="field"><label for="d-kcal">Ккал</label><input type="number" id="d-kcal" step="any" min="0" /></div>
          <div class="field"><label for="d-p">Белки, г</label><input type="number" id="d-p" step="any" min="0" /></div>
          <div class="field"><label for="d-f">Жиры, г</label><input type="number" id="d-f" step="any" min="0" /></div>
          <div class="field"><label for="d-c">Углеводы, г</label><input type="number" id="d-c" step="any" min="0" /></div>
        </div>
      </fieldset>
      <div class="field" style="grid-column:1/-1">
        <span class="hint">Дополнительные флаги (доступны только если все продукты в составе с соответствующим флагом)</span>
        <div class="stack" style="margin-top:0.35rem" id="d-flag-box">
          ${DIETARY_FLAG_LABELS.map(
            (f) => `
            <label><input type="checkbox" name="dflag_${f.key}" data-bit="${f.bit}" /> ${escapeHtml(f.label)}</label>
          `
          ).join("")}
        </div>
      </div>
      <div class="field" style="grid-column:1/-1">
        <button type="submit" class="btn btn--primary">${isEdit ? "Сохранить" : "Создать"}</button>
      </div>
    </form>
  `;

  const form = root.querySelector("#dform");
  const errEl = root.querySelector("#dform-err");
  const compHost = root.querySelector("#d-comp-rows");
  const calcNowEl = root.querySelector("#d-calc-now");

  function getCompositionIds() {
    const rows = compHost.querySelectorAll("[data-row]");
    const ids = [];
    rows.forEach((row) => {
      const v = row.querySelector(".js-prod")?.value;
      if (v) ids.push(v);
    });
    return ids;
  }

  function refreshFlagsUi() {
    const allowed = allowedFlagsFromProducts(productById, getCompositionIds());
    const current = flagsFromCheckboxes(root);
    setDishFlags(root, current, allowed);
  }

  function computeNow() {
    const rows = compHost.querySelectorAll("[data-row]");
    let kcal = 0, p = 0, f = 0, c = 0;
    let hasAny = false;

    for (const row of rows) {
      const pid = row.querySelector(".js-prod")?.value;
      const grams = Number(row.querySelector(".js-qty")?.value);
      if (!pid) continue;
      if (!Number.isFinite(grams) || grams <= 0) continue;
      const prod = productById.get(pid);
      if (!prod) continue;

      hasAny = true;
      const factor = grams / 100;
      kcal += Number(prod.caloriesPer100g || 0) * factor;
      p += Number(prod.proteinsPer100g || 0) * factor;
      f += Number(prod.fatsPer100g || 0) * factor;
      c += Number(prod.carbsPer100g || 0) * factor;
    }

    if (!hasAny) {
      calcNowEl.textContent = "";
      return;
    }

    const r = (x) => Math.round(x * 10) / 10;
    calcNowEl.textContent = `расчет сейчас: ${r(kcal)} ккал, б ${r(p)} г, ж ${r(f)} г, у ${r(c)} г`;
  }

  function addRow(prefill = { productId: "", quantityGrams: "" }) {
    const wrap = document.createElement("div");
    wrap.innerHTML = compositionRowHtml(productOptions);
    const row = wrap.firstElementChild;
    compHost.appendChild(row);
    if (prefill.productId) row.querySelector(".js-prod").value = prefill.productId;
    if (prefill.quantityGrams !== "")
      row.querySelector(".js-qty").value = String(prefill.quantityGrams);
    row.querySelector(".js-remove").addEventListener("click", () => {
      row.remove();
      if (!compHost.querySelector("[data-row]")) addRow();
      refreshFlagsUi();
      computeNow();
    });
    row.querySelector(".js-prod").addEventListener("change", () => {
      refreshFlagsUi();
      computeNow();
    });
    row.querySelector(".js-qty").addEventListener("input", () => {
      refreshFlagsUi();
      computeNow();
    });
  }

  compHost.innerHTML = "";
  if (existing?.ingredients?.length) {
    for (const line of existing.ingredients) {
      addRow({ productId: line.productId, quantityGrams: line.quantityGrams });
    }
  } else {
    addRow();
  }

  root.querySelector("#d-add-row").addEventListener("click", () => addRow());

  if (existing) {
    form.querySelector("#d-name").value = existing.name || "";
    form.querySelector("#d-photos").value = (existing.photoUrls || []).join("\n");
    form.querySelector("#d-portion").value = String(existing.portionSizeGrams ?? "");
    form.querySelector("#d-cat").value = existing.category || "";
    if (existing.caloriesPerPortion != null)
      form.querySelector("#d-kcal").value = String(existing.caloriesPerPortion);
    if (existing.proteinsPerPortion != null)
      form.querySelector("#d-p").value = String(existing.proteinsPerPortion);
    if (existing.fatsPerPortion != null)
      form.querySelector("#d-f").value = String(existing.fatsPerPortion);
    if (existing.carbsPerPortion != null)
      form.querySelector("#d-c").value = String(existing.carbsPerPortion);
    setDishFlags(root, existing.additionalFlags, existing.allowedAdditionalFlags);
  } else {
    refreshFlagsUi();
  }
  computeNow();

  form.addEventListener("submit", async (ev) => {
    ev.preventDefault();
    errEl.innerHTML = "";

    const rows = compHost.querySelectorAll("[data-row]");
    /** @type {{ productId: string, quantityGrams: number }[]} */
    const composition = [];
    for (const row of rows) {
      const pid = row.querySelector(".js-prod")?.value;
      const qty = Number(row.querySelector(".js-qty")?.value);
      if (!pid) continue;
      if (!Number.isFinite(qty) || qty <= 0) {
        errEl.innerHTML = `<div class="alert alert--error">Укажите положительное количество для каждой строки состава.</div>`;
        return;
      }
      composition.push({ productId: pid, quantityGrams: qty });
    }
    if (composition.length === 0) {
      errEl.innerHTML = `<div class="alert alert--error">Добавьте хотя бы один продукт в состав.</div>`;
      return;
    }

    /** @type {Record<string, unknown>} */
    const body = {
      name: form.querySelector("#d-name").value.trim(),
      photoUrls: parsePhotos(form.querySelector("#d-photos").value),
      portionSizeGrams: Number(form.querySelector("#d-portion").value),
      composition: composition.map((c) => ({
        productId: c.productId,
        quantityGrams: c.quantityGrams
      })),
      additionalFlags: flagsFromCheckboxes(root)
    };

    const cat = form.querySelector("#d-cat").value;
    if (cat) body.category = cat;

    const numOrNull = (id) => {
      const el = form.querySelector(id);
      const v = el?.value?.trim();
      if (v === "" || v == null) return undefined;
      const n = Number(v);
      return Number.isFinite(n) ? n : undefined;
    };
    const kcal = numOrNull("#d-kcal");
    const p = numOrNull("#d-p");
    const f = numOrNull("#d-f");
    const c = numOrNull("#d-c");
    if (kcal !== undefined) body.caloriesPerPortion = kcal;
    if (p !== undefined) body.proteinsPerPortion = p;
    if (f !== undefined) body.fatsPerPortion = f;
    if (c !== undefined) body.carbsPerPortion = c;

    try {
      if (isEdit) {
        await api.put(`/api/dishes/${id}`, body);
        window.location.hash = `#/dishes/${id}`;
      } else {
        const created = await api.post("/api/dishes", body);
        window.location.hash = `#/dishes/${created.id}`;
      }
    } catch (e) {
      const msg = e instanceof ApiError ? e.message : String(e);
      errEl.innerHTML = `<div class="alert alert--error">${escapeHtml(msg)}</div>`;
    }
  });
}
