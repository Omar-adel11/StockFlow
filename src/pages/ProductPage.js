import { productService } from '../services/ProductService.js';
import { getAllCategories } from '../services/categoryService.js';
import { supplierService } from '../services/SupplierService.js';
import { validateProductForm, buildProductPayload } from '../validation/productValidation.js';
import { getAccessToken, clearSession } from '../sessions/session.js';
import {
  productState,
  setProducts,
  getProducts,
  addProductToState,
  updateProductInState,
  removeProductFromState,
  setCategories,
  setSuppliers
} from '../state/productState.js';

class ProductsPage {
  constructor() {
    this.editingId = null;
    this.initElements();
    this.bindEvents();
    this.init();
  }

  initElements() {
    this.logoutBtn = document.getElementById('logout-btn');

    this.form = document.getElementById('product-form');
    this.formTitle = document.getElementById('form-title');
    this.submitBtn = document.getElementById('submit-btn');
    this.cancelBtn = document.getElementById('cancel-btn');
    this.formStatus = document.getElementById('form-status');

    this.skuInput = document.getElementById('product-sku');
    this.nameInput = document.getElementById('product-name');
    this.categorySelect = document.getElementById('product-category');
    this.supplierSelect = document.getElementById('product-supplier');
    this.unitPriceInput = document.getElementById('product-unit-price');
    this.reorderLevelInput = document.getElementById('product-reorder-level');
    this.isActiveCheckbox = document.getElementById('product-is-active');

    this.productsList = document.getElementById('products-list');
    this.productsEmpty = document.getElementById('products-empty');
    this.productsLoading = document.getElementById('products-loading');
  }

  bindEvents() {
    if (this.logoutBtn) {
      this.logoutBtn.addEventListener('click', () => {
        clearSession();
        window.location.href = 'login.html';
      });
    }

    if (this.form) {
      this.form.addEventListener('submit', (e) => this.handleSubmit(e));
    }

    if (this.cancelBtn) {
      this.cancelBtn.addEventListener('click', () => this.resetForm());
    }
  }

  async init() {
    const token = getAccessToken();
    if (!token) {
      window.location.href = 'login.html';
      return;
    }

    await Promise.all([
      this.loadCategories(),
      this.loadSuppliers(),
      this.loadProducts()
    ]);
  }

  async loadCategories() {
    try {
      const categories = await getAllCategories();
      setCategories(categories);
      this.populateSelect(this.categorySelect, categories, 'Select Category', false);
    } catch (error) {
      console.error('Failed to load categories:', error);
      this.categorySelect.innerHTML = '<option value="">Failed to load categories</option>';
    }
  }

  async loadSuppliers() {
    try {
      const suppliers = await supplierService.getAll();
      setSuppliers(suppliers);
      this.populateSelect(this.supplierSelect, suppliers, 'None', true);
    } catch (error) {
      console.error('Failed to load suppliers:', error);
      this.supplierSelect.innerHTML = '<option value="">None</option>';
    }
  }

  populateSelect(selectEl, items, defaultLabel, allowEmpty = false) {
    if (!selectEl) return;
    selectEl.innerHTML = allowEmpty 
      ? `<option value="">${defaultLabel}</option>`
      : `<option value="">${defaultLabel}</option>`;

    items.forEach(item => {
      const opt = document.createElement('option');
      opt.value = item.id;
      opt.textContent = item.name || item.title || `ID: ${item.id}`;
      selectEl.appendChild(opt);
    });
  }

  async loadProducts() {
    this.showLoading(true);
    try {
      const products = await productService.getAll();
      setProducts(products);
      this.renderProducts();
    } catch (error) {
      this.setStatus('Failed to load products: ' + error.message, 'error');
    } finally {
      this.showLoading(false);
    }
  }

  renderProducts() {
    const products = getProducts();
    this.productsList.innerHTML = '';

    if (!products || products.length === 0) {
      this.productsEmpty.hidden = false;
      return;
    }

    this.productsEmpty.hidden = true;

    products.forEach((product) => {
      const card = document.createElement('div');
      card.className = 'card entity-card';

      const skuVal = product.itemSKU || product.ItemSKU || product.sku || 'N/A';
      const categoryName = product.categoryName || product.category?.name || 'Uncategorized';
      const supplierName = product.preferredSupplierName || product.preferredSupplier?.name || 'None';
      
      // Read price using unitSellingPrice / UnitSellingPrice / unitPrice
      const rawPrice = product.unitSellingPrice ?? product.UnitSellingPrice ?? product.unitPrice;
      const formattedPrice = typeof rawPrice === 'number' ? `$${rawPrice.toFixed(2)}` : '$0.00';

      card.innerHTML = `
        <div class="entity-card-header">
          <h3>${this.escapeHtml(product.name)}</h3>
          <span class="badge ${product.isActive ? 'badge-success' : 'badge-danger'}">
            ${product.isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
        <div class="entity-card-body">
          <p><strong>SKU:</strong> ${this.escapeHtml(skuVal)}</p>
          <p><strong>Category:</strong> ${this.escapeHtml(categoryName)}</p>
          <p><strong>Preferred Supplier:</strong> ${this.escapeHtml(supplierName)}</p>
          <p><strong>Selling Price:</strong> ${formattedPrice}</p>
          <p><strong>Reorder Level:</strong> ${product.reorderLevel ?? 0}</p>
        </div>
        <div class="entity-card-actions">
          <button type="button" class="btn btn-secondary btn-sm edit-btn">Edit</button>
          <button type="button" class="btn btn-danger btn-sm delete-btn">Delete</button>
        </div>
      `;

      card.querySelector('.edit-btn').addEventListener('click', () => this.startEdit(product));
      card.querySelector('.delete-btn').addEventListener('click', () => this.deleteProduct(product.id));

      this.productsList.appendChild(card);
    });
  }

  async handleSubmit(event) {
  event.preventDefault();
  this.setStatus('');

  const rawFormData = {
    sku: this.skuInput.value,
    name: this.nameInput.value,
    categoryId: this.categorySelect.value,
    preferredSupplierId: this.supplierSelect.value,
    unitPrice: this.unitPriceInput.value,
    unitSellingPrice: this.unitPriceInput.value,
    reorderLevel: this.reorderLevelInput.value,
    isActive: this.isActiveCheckbox.checked
  };

  const validation = validateProductForm(rawFormData);
  if (!validation.isValid) {
    this.setStatus(validation.errors.join(' '), 'error');
    return;
  }

  const isEdit = Boolean(this.editingId);
  const payload = buildProductPayload(rawFormData, isEdit);

  try {
    this.submitBtn.disabled = true;

    if (this.editingId) {
      const updatedProduct = await productService.update(this.editingId, payload);
      updateProductInState(updatedProduct || { id: this.editingId, ...payload });
      this.setStatus('Product updated successfully.', 'success');
    } else {
      const newProduct = await productService.create(payload);
      addProductToState(newProduct || payload);
      this.setStatus('Product created successfully.', 'success');
    }

    this.resetForm();
    await this.loadProducts();
  } catch (error) {
    console.error('API Error details:', error);
    this.setStatus(error.message || 'An error occurred while saving product.', 'error');
  } finally {
    this.submitBtn.disabled = false;
  }
}

  startEdit(product) {
    this.editingId = product.id;

    this.skuInput.value = product.itemSKU || product.ItemSKU || product.sku || '';
    this.nameInput.value = product.name || '';
    this.categorySelect.value = product.categoryId || (product.category ? product.category.id : '');
    this.supplierSelect.value = product.preferredSupplierId || (product.preferredSupplier ? product.preferredSupplier.id : '');
    
    // Read price into form input
    this.unitPriceInput.value = product.unitSellingPrice ?? product.UnitSellingPrice ?? product.unitPrice ?? '';
    this.reorderLevelInput.value = product.reorderLevel ?? 0;
    this.isActiveCheckbox.checked = product.isActive !== undefined ? product.isActive : true;

    this.formTitle.textContent = 'Edit Product';
    this.submitBtn.textContent = 'Update Product';
    this.cancelBtn.hidden = false;
    this.nameInput.focus();
  }

  async deleteProduct(id) {
    if (!confirm('Are you sure you want to delete this product?')) return;

    try {
      await productService.delete(id);
      removeProductFromState(id);
      this.setStatus('Product deleted successfully.', 'success');
      this.renderProducts();
    } catch (error) {
      this.setStatus('Failed to delete product: ' + error.message, 'error');
    }
  }

  resetForm() {
    this.editingId = null;
    this.form.reset();
    this.isActiveCheckbox.checked = true;
    this.formTitle.textContent = 'Add New Product';
    this.submitBtn.textContent = 'Save Product';
    this.cancelBtn.hidden = true;
  }

  showLoading(isLoading) {
    if (this.productsLoading) {
      this.productsLoading.hidden = !isLoading;
    }
  }

  setStatus(message, type = '') {
    if (!this.formStatus) return;
    this.formStatus.textContent = message;
    this.formStatus.className = `form-status ${type}`;
  }

  escapeHtml(str) {
    return String(str || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }
}

document.addEventListener('DOMContentLoaded', () => {
  new ProductsPage();
});