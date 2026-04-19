import { closeModal } from "./ui/modal.js";
import { renderHome } from "./pages/home.js";
import { renderProductsList } from "./pages/productsList.js";
import { renderProductForm } from "./pages/productForm.js";
import { renderProductDetail } from "./pages/productDetail.js";
import { renderDishesList } from "./pages/dishesList.js";
import { renderDishForm } from "./pages/dishForm.js";
import { renderDishDetail } from "./pages/dishDetail.js";

const main = () => document.getElementById("main");

function updateNav(hash) {
  const h = (hash || "#/").replace(/^#/, "");
  const seg = h.split("/").filter(Boolean)[0] || "";
  document.querySelectorAll("#main-nav a[data-path]").forEach((a) => {
    const p = a.getAttribute("data-path") ?? "";
    const cur = p === seg;
    if (cur) a.setAttribute("aria-current", "page");
    else a.removeAttribute("aria-current");
  });
}

async function route() {
  closeModal();
  const root = main();
  if (!root) return;

  let hash = window.location.hash || "#/";
  if (hash === "#") hash = "#/";

  /** @type {RegExp} */
  const routes = [
    [/^#\/products\/new\/?$/, async () => renderProductForm(root, null)],
    [/^#\/products\/([0-9a-f-]{36})\/edit\/?$/i, async (m) => renderProductForm(root, m[1])],
    [/^#\/products\/([0-9a-f-]{36})\/?$/i, async (m) => renderProductDetail(root, m[1])],
    [/^#\/products\/?$/, async () => renderProductsList(root)],
    [/^#\/dishes\/new\/?$/, async () => renderDishForm(root, null)],
    [/^#\/dishes\/([0-9a-f-]{36})\/edit\/?$/i, async (m) => renderDishForm(root, m[1])],
    [/^#\/dishes\/([0-9a-f-]{36})\/?$/i, async (m) => renderDishDetail(root, m[1])],
    [/^#\/dishes\/?$/, async () => renderDishesList(root)],
    [/^#\/?$/, async () => renderHome(root)]
  ];

  updateNav(hash);

  for (const [re, fn] of routes) {
    const m = re.exec(hash);
    if (m) {
      root.focus();
      await fn(m);
      return;
    }
  }

  root.innerHTML = `<div class="alert alert--info">Страница не найдена. <a href="#/">На главную</a></div>`;
}

window.addEventListener("hashchange", () => route());

if (!window.location.hash || window.location.hash === "#") {
  window.location.hash = "#/";
} else {
  route();
}
