export const inventoryState = {
  items: [],
  movements: [],
  products: [],
  warehouses: [],
  filters: {
    productId: '',
    warehouseId: '',
    lowStockOnly: false
  }
};

export function setInventoryItems(items) {
  inventoryState.items = Array.isArray(items) ? items : [];
}

export function setStockMovements(movements) {
  inventoryState.movements = Array.isArray(movements) ? movements : [];
}

export function setProducts(products) {
  inventoryState.products = Array.isArray(products) ? products : [];
}

export function setWarehouses(warehouses) {
  inventoryState.warehouses = Array.isArray(warehouses) ? warehouses : [];
}

export function updateFilters(newFilters) {
  inventoryState.filters = { ...inventoryState.filters, ...newFilters };
}

export function getFilteredItems() {
  let filtered = [...inventoryState.items];
  const { productId, warehouseId, lowStockOnly } = inventoryState.filters;

  if (productId) {
    filtered = filtered.filter(item => item.productId == productId);
  }

  if (warehouseId) {
    filtered = filtered.filter(item => item.warehouseId == warehouseId);
  }

  if (lowStockOnly) {
    filtered = filtered.filter(item => item.quantityOnHand <= (item.reorderLevel ?? 0));
  }

  return filtered;
}