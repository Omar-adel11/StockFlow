export const warehouseState = {
  warehouses: []
};

export function setWarehouses(warehouses) {
  warehouseState.warehouses = Array.isArray(warehouses) ? warehouses : [];
}

export function getWarehouses() {
  return warehouseState.warehouses;
}

export function addWarehouseToState(warehouse) {
  warehouseState.warehouses.unshift(warehouse);
}

export function updateWarehouseInState(updatedWarehouse) {
  const index = warehouseState.warehouses.findIndex(w => w.id === updatedWarehouse.id);
  if (index !== -1) {
    warehouseState.warehouses[index] = { ...warehouseState.warehouses[index], ...updatedWarehouse };
  }
}

export function removeWarehouseFromState(id) {
  warehouseState.warehouses = warehouseState.warehouses.filter(w => w.id !== id);
}