export const supplierState = {
  suppliers: []
};

export function setSuppliers(suppliers) {
  supplierState.suppliers = Array.isArray(suppliers) ? suppliers : [];
}

export function getSuppliers() {
  return supplierState.suppliers;
}

export function addSupplierToState(supplier) {
  supplierState.suppliers.unshift(supplier);
}

export function updateSupplierInState(updatedSupplier) {
  const index = supplierState.suppliers.findIndex(s => s.id === updatedSupplier.id);
  if (index !== -1) {
    supplierState.suppliers[index] = { ...supplierState.suppliers[index], ...updatedSupplier };
  }
}

export function removeSupplierFromState(id) {
  supplierState.suppliers = supplierState.suppliers.filter(s => s.id !== id);
}