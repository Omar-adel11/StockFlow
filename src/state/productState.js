export const productState = {
  products: [],
  categories: [],
  suppliers: []
};

export function setProducts(products) {
  productState.products = Array.isArray(products) ? products : [];
}

export function getProducts() {
  return productState.products;
}

export function addProductToState(product) {
  productState.products.unshift(product);
}

export function updateProductInState(updatedProduct) {
  const index = productState.products.findIndex(p => p.id === updatedProduct.id);
  if (index !== -1) {
    productState.products[index] = { ...productState.products[index], ...updatedProduct };
  }
}

export function removeProductFromState(id) {
  productState.products = productState.products.filter(p => p.id !== id);
}

export function setCategories(categories) {
  productState.categories = Array.isArray(categories) ? categories : [];
}

export function setSuppliers(suppliers) {
  productState.suppliers = Array.isArray(suppliers) ? suppliers : [];
}