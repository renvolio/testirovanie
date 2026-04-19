/** Лейблы enum как на бэкенде (строковые имена в JSON) */

export const PRODUCT_CATEGORY = {
  Frozen: "Замороженный",
  Meat: "Мясной",
  Vegetables: "Овощи",
  Greens: "Зелень",
  Spices: "Специи",
  Grains: "Крупы",
  Canned: "Консервы",
  Liquid: "Жидкость",
  Sweets: "Сладости"
};

export const COOKING = {
  ReadyToEat: "Готовый к употреблению",
  SemiFinished: "Полуфабрикат",
  RequiresCooking: "Требует приготовления"
};

export const DISH_CATEGORY = {
  Dessert: "Десерт",
  FirstCourse: "Первое",
  SecondCourse: "Второе",
  Drink: "Напиток",
  Salad: "Салат",
  Soup: "Суп",
  Snack: "Перекус"
};

export const DIETARY_FLAG_LABELS = [
  { bit: 1, key: "Vegan", label: "Веган" },
  { bit: 2, key: "GlutenFree", label: "Без глютена" },
  { bit: 4, key: "SugarFree", label: "Без сахара" }
];

/** @param {number | string} flags */
export function formatDietaryFlags(flags) {
  const n = typeof flags === "string" ? parseEnumFlags(flags) : Number(flags);
  if (!n) return "—";
  const parts = [];
  for (const { bit, label } of DIETARY_FLAG_LABELS) {
    if (n & bit) parts.push(label);
  }
  return parts.length ? parts.join(", ") : "—";
}

/** Парсит "Vegan, GlutenFree" или число */
function parseEnumFlags(s) {
  if (typeof s === "number") return s;
  let v = 0;
  const parts = String(s).split(",").map((x) => x.trim());
  for (const p of parts) {
    const f = DIETARY_FLAG_LABELS.find((x) => x.key === p);
    if (f) v |= f.bit;
  }
  return v;
}

export const PRODUCT_SORT = [
  { value: "name", label: "Название" },
  { value: "calories", label: "Калорийность" },
  { value: "proteins", label: "Белки" },
  { value: "fats", label: "Жиры" },
  { value: "carbs", label: "Углеводы" }
];
