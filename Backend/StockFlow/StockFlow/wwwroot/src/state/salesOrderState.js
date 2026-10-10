export const salesOrderState = {
  orders: [],
  customers: [],
  warehouses: [],
  products: [],
  lineItems: []
};

export function setOrders(orders) {
  salesOrderState.orders = Array.isArray(orders) ? orders : [];
}

export function setCustomers(customers) {
  salesOrderState.customers = Array.isArray(customers) ? customers : [];
}

export function setWarehouses(warehouses) {
  salesOrderState.warehouses = Array.isArray(warehouses) ? warehouses : [];
}

export function setProducts(products) {
  salesOrderState.products = Array.isArray(products) ? products : [];
}

export function addLineItem(item = { productId: '', quantitySold: 1, billedUnitPrice: 0 }) {
  salesOrderState.lineItems.push({
    id: Date.now() + Math.random(),
    ...item
  });
}

export function updateLineItem(id, fields) {
  const item = salesOrderState.lineItems.find(i => i.id === id);
  if (item) {
    Object.assign(item, fields);
  }
}

export function removeLineItem(id) {
  salesOrderState.lineItems = salesOrderState.lineItems.filter(i => i.id !== id);
}

export function clearLineItems() {
  salesOrderState.lineItems = [];
}

export function calculateGrandTotal() {
  return salesOrderState.lineItems.reduce((sum, item) => {
    const qty = parseInt(item.quantitySold, 10) || 0;
    const price = parseFloat(item.billedUnitPrice) || 0;
    return sum + (qty * price);
  }, 0);
}