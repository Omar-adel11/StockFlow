export const purchaseOrderState = {
  orders: [],
  suppliers: [],
  warehouses: [],
  products: [],
  lineItems: []
};

export function setOrders(orders) {
  purchaseOrderState.orders = Array.isArray(orders) ? orders : [];
}

export function setSuppliers(suppliers) {
  purchaseOrderState.suppliers = Array.isArray(suppliers) ? suppliers : [];
}

export function setWarehouses(warehouses) {
  purchaseOrderState.warehouses = Array.isArray(warehouses) ? warehouses : [];
}

export function setProducts(products) {
  purchaseOrderState.products = Array.isArray(products) ? products : [];
}

export function addLineItem(item = { productId: '', quantityOrdered: 1, agreedUnitPrice: 0 }) {
  purchaseOrderState.lineItems.push({
    id: Date.now() + Math.random(),
    ...item
  });
}

export function updateLineItem(id, fields) {
  const item = purchaseOrderState.lineItems.find(i => i.id === id);
  if (item) {
    Object.assign(item, fields);
  }
}

export function removeLineItem(id) {
  purchaseOrderState.lineItems = purchaseOrderState.lineItems.filter(i => i.id !== id);
}

export function clearLineItems() {
  purchaseOrderState.lineItems = [];
}

export function calculateGrandTotal() {
  return purchaseOrderState.lineItems.reduce((sum, item) => {
    const qty = parseInt(item.quantityOrdered, 10) || 0;
    const price = parseFloat(item.agreedUnitPrice) || 0;
    return sum + (qty * price);
  }, 0);
}