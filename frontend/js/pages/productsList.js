import { api, ApiError } from "../api/client.js";
import {
  PRODUCT_CATEGORY,
  COOKING,
  PRODUCT_SORT
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

function sortOptions() {
  return PRODUCT_SORT.map(
    (o) => `<option value="${escapeHtml(o.value)}">${escapeHtml(o.label)}</option>`
  ).join("");
}

export async function renderProductsList(root) {
  root.innerHTML = `
    <h1 class="page-title">Продукты</h1>
    <div class="toolbar">
      <a class="btn btn--primary" href="#/products/new">Новый продукт</a>
    </div>
    <form class="card filters" id="pfilters" aria-label="Фильтры списка продуктов">
      <div class="field">
        <label for="f-search">Поиск по названию</label>
        <input type="text" id="f-search" name="search" autocomplete="off" />
      </div>
      <div class="field">
        <label for="f-cat">Категория</label>
        <select id="f-cat" name="category">
          <option value="">— любая —</option>
          ${categoryOptions()}
        </select>
      </div>
      <div class="field">
        <label for="f-cook">Готовность</label>
        <select id="f-cook" name="cookingRequirement">
          <option value="">— любая —</option>
          ${cookingOptions()}
        </select>
      </div>
      <div class="field">
        <label><input type="checkbox" name="vegan" value="true" /> Веган</label>
      </div>
      <div class="field">
        <label><input type="checkbox" name="glutenFree" value="true" /> Без глютена</label>
      </div>
      <div class="field">
        <label><input type="checkbox" name="sugarFree" value="true" /> Без сахара</label>
      </div>
      <div class="field">
        <label for="f-sort">Сортировка</label>
        <select id="f-sort" name="sortBy">${sortOptions()}</select>
      </div>
      <div class="field">
        <label><input type="checkbox" name="sortDescending" value="true" /> По убыванию</label>
      </div>
      <div class="field" style="align-self:flex-end">
        <button type="submit" class="btn btn--secondary">Применить</button>
      </div>
    </form>
    <div id="plist-out"></div>
  `;

  const out = root.querySelector("#plist-out");
  const form = root.querySelector("#pfilters");

  async function load() {
    const fd = new FormData(form);
    /** @type {Record<string, string|boolean>} */
    const query = {};
    const search = fd.get("search");
    if (search) query.search = String(search).trim();
    const cat = fd.get("category");
    if (cat) query.category = String(cat);
    const cook = fd.get("cookingRequirement");
    if (cook) query.cookingRequirement = String(cook);
    if (fd.get("vegan")) query.vegan = true;
    if (fd.get("glutenFree")) query.glutenFree = true;
    if (fd.get("sugarFree")) query.sugarFree = true;
    const sortBy = fd.get("sortBy");
    if (sortBy) query.sortBy = String(sortBy);
    if (fd.get("sortDescending")) query.sortDescending = true;

    out.innerHTML = `<p class="empty-state"><span class="spinner" aria-hidden="true"></span> Загрузка…</p>`;
    try {
      const items = await api.get("/api/products", query);
      if (!Array.isArray(items) || items.length === 0) {
        out.innerHTML = `<p class="empty-state">Ничего не найдено.</p>`;
        return;
      }
      out.innerHTML = `
        <div class="table-wrap card" style="padding:0">
          <table class="data">
            <thead>
              <tr>
                <th>Название</th>
                <th>Категория</th>
                <th>Ккал/100г</th>
                <th>Б/Ж/У</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              ${items
                .map(
                  (p) => `
                <tr>
                  <td><a href="#/products/${p.id}">${escapeHtml(p.name)}</a></td>
                  <td>${escapeHtml(PRODUCT_CATEGORY[p.category] || p.category)}</td>
                  <td>${escapeHtml(String(p.caloriesPer100g))}</td>
                  <td>${p.proteinsPer100g}/${p.fatsPer100g}/${p.carbsPer100g}</td>
                  <td><a class="btn btn--sm btn--ghost" href="#/products/${p.id}/edit">Изменить</a></td>
                </tr>
              `
                )
                .join("")}
            </tbody>
          </table>
        </div>
      `;
    } catch (e) {
      const msg = e instanceof ApiError ? e.message : String(e);
      out.innerHTML = `<div class="alert alert--error">${escapeHtml(msg)}</div>`;
    }
  }

  form.addEventListener("submit", (ev) => {
    ev.preventDefault();
    load();
  });

  await load();
}
