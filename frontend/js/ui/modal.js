import { escapeHtml } from "../utils/html.js";

const host = () => document.getElementById("modal-host");

export function closeModal() {
  const h = host();
  if (!h) return;
  h.hidden = true;
  h.innerHTML = "";
  h.removeAttribute("role");
}

/**
 * @param {{ title: string, bodyHtml: string, actionsHtml?: string }} opts
 */
export function openModal(opts) {
  const h = host();
  if (!h) return;
  h.innerHTML = `
    <div class="modal" role="dialog" aria-modal="true" aria-labelledby="modal-title">
      <h2 id="modal-title">${escapeHtml(opts.title)}</h2>
      <div class="modal-body">${opts.bodyHtml}</div>
      ${opts.actionsHtml || `<div class="modal-actions"><button type="button" class="btn btn--secondary js-close">Закрыть</button></div>`}
    </div>
  `;
  h.hidden = false;
  h.setAttribute("role", "dialog");
  h.querySelector(".js-close")?.addEventListener("click", closeModal);
  h.addEventListener("click", (e) => {
    if (e.target === h) closeModal();
  });
  const onKey = (e) => {
    if (e.key === "Escape") {
      closeModal();
      document.removeEventListener("keydown", onKey);
    }
  };
  document.addEventListener("keydown", onKey);
  const btn = h.querySelector("button, [href]");
  btn?.focus();
}
