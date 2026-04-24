import { api, ApiError } from "../api/client.js";
import { DISH_CATEGORY, formatDietaryFlags } from "../constants/labels.js";
import { escapeHtml } from "../utils/html.js";

function dishCategoryOptions() {
  return Object.entries(DISH_CATEGORY)
    .map(([v, l]) => `<option value="${escapeHtml(v)}">${escapeHtml(l)}</option>`)
    .join("");
}

export async function renderDishesList(root) {
  root.innerHTML = `
    <h1 class="page-title">Блюда</h1>
    <div class="toolbar">
      <a class="btn btn--primary" href="#/dishes/new">Новое блюдо</a>
    </div>
    <form class="card filters" id="dfilters" aria-label="Фильтры списка блюд">
      <div class="field">
        <label for="d-search">Поиск по названию</label>
        <input type="text" id="d-search" name="search" />
      </div>
      <div class="field">
        <label for="d-cat">Категория</label>
        <select id="d-cat" name="category">
          <option value="">— любая —</option>
          ${dishCategoryOptions()}
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
      <div class="field" style="align-self:flex-end">
        <button type="submit" class="btn btn--secondary">Применить</button>
      </div>
    </form>
    <div id="dlist-out"></div>
  `;

  const out = root.querySelector("#dlist-out");
  const form = root.querySelector("#dfilters");

  async function load() {
    const fd = new FormData(form);
    /** @type {Record<string, string|boolean>} */
    const query = {};
    const s = fd.get("search");
    if (s) query.search = String(s).trim();
    const cat = fd.get("category");
    if (cat) query.category = String(cat);
    if (fd.get("vegan")) query.vegan = true;
    if (fd.get("glutenFree")) query.glutenFree = true;
    if (fd.get("sugarFree")) query.sugarFree = true;

    out.innerHTML = `<p class="empty-state"><span class="spinner"></span> Загрузка…</p>`;
    try {
      const items = await api.get("/api/dishes", query);
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
                <th>Порция, г</th>
                <th>Ккал/порция</th>
                <th>Флаги</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              ${items
                .map(
                  (d) => `
                <tr>
                  <td><a href="#/dishes/${d.id}">${escapeHtml(d.name)}</a></td>
                  <td>${escapeHtml(DISH_CATEGORY[d.category] || d.category)}</td>
                  <td>${escapeHtml(String(d.portionSizeGrams))}</td>
                  <td>${escapeHtml(String(d.caloriesPerPortion))}</td>
                  <td>${escapeHtml(formatDietaryFlags(d.additionalFlags))}</td>
                  <td><a class="btn btn--sm btn--ghost" href="#/dishes/${d.id}/edit">Изменить</a></td>
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
