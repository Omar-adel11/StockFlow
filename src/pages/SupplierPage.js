import { supplierService } from '../services/SupplierService.js';
import { validateSupplierForm, buildSupplierPayload } from '../validation/supplierValidation.js';
import { getAccessToken, clearSession } from '../sessions/session.js';
import {
  setSuppliers,
  getSuppliers,
  addSupplierToState,
  updateSupplierInState,
  removeSupplierFromState
} from '../state/supplierState.js';

class SuppliersPage {
  constructor() {
    this.editingId = null;
    this.initElements();
    this.bindEvents();
    this.init();
  }

  initElements() {
    this.logoutBtn = document.getElementById('logout-btn');

    this.form = document.getElementById('supplier-form');
    this.formTitle = document.getElementById('form-title');
    this.submitBtn = document.getElementById('submit-btn');
    this.cancelBtn = document.getElementById('cancel-btn');
    this.formStatus = document.getElementById('form-status');

    this.nameInput = document.getElementById('supplier-name');
    this.emailInput = document.getElementById('supplier-email');
    this.phoneInput = document.getElementById('supplier-phone');
    this.addressInput = document.getElementById('supplier-address');
    this.isActiveCheckbox = document.getElementById('supplier-is-active');

    this.suppliersList = document.getElementById('suppliers-list');
    this.suppliersEmpty = document.getElementById('suppliers-empty');
    this.suppliersLoading = document.getElementById('suppliers-loading');
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

    await this.loadSuppliers();
  }

  async loadSuppliers() {
    this.showLoading(true);
    try {
      const suppliers = await supplierService.getAll();
      setSuppliers(suppliers);
      this.renderSuppliers();
    } catch (error) {
      this.setStatus('Failed to load suppliers: ' + error.message, 'error');
    } finally {
      this.showLoading(false);
    }
  }

  renderSuppliers() {
    const suppliers = getSuppliers();
    this.suppliersList.innerHTML = '';

    if (!suppliers || suppliers.length === 0) {
      this.suppliersEmpty.hidden = false;
      return;
    }

    this.suppliersEmpty.hidden = true;

    suppliers.forEach((supplier) => {
      const card = document.createElement('div');
      card.className = 'card entity-card';

      const email = supplier.contactEmail || 'N/A';
      const phone = supplier.contactPhone || 'N/A';
      const address = supplier.address || 'N/A';

      card.innerHTML = `
        <div class="entity-card-header">
          <h3>${this.escapeHtml(supplier.name)}</h3>
          <span class="badge ${supplier.isActive ? 'badge-success' : 'badge-danger'}">
            ${supplier.isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
        <div class="entity-card-body">
          <p><strong>Email:</strong> ${this.escapeHtml(email)}</p>
          <p><strong>Phone:</strong> ${this.escapeHtml(phone)}</p>
          <p><strong>Address:</strong> ${this.escapeHtml(address)}</p>
        </div>
        <div class="entity-card-actions">
          <button type="button" class="btn btn-secondary btn-sm edit-btn">Edit</button>
          <button type="button" class="btn btn-danger btn-sm delete-btn">Delete</button>
        </div>
      `;

      card.querySelector('.edit-btn').addEventListener('click', () => this.startEdit(supplier));
      card.querySelector('.delete-btn').addEventListener('click', () => this.deleteSupplier(supplier.id));

      this.suppliersList.appendChild(card);
    });
  }

  async handleSubmit(event) {
    event.preventDefault();
    this.setStatus('');

    const rawFormData = {
      name: this.nameInput.value,
      contactEmail: this.emailInput.value,
      contactPhone: this.phoneInput.value,
      address: this.addressInput.value,
      isActive: this.isActiveCheckbox.checked
    };

    const validation = validateSupplierForm(rawFormData);
    if (!validation.isValid) {
      this.setStatus(validation.errors.join(' '), 'error');
      return;
    }

    const payload = buildSupplierPayload(rawFormData);

    try {
      this.submitBtn.disabled = true;

      if (this.editingId) {
        const updatedSupplier = await supplierService.update(this.editingId, payload);
        updateSupplierInState(updatedSupplier || { id: this.editingId, ...payload });
        this.setStatus('Supplier updated successfully.', 'success');
      } else {
        const newSupplier = await supplierService.create(payload);
        addSupplierToState(newSupplier || payload);
        this.setStatus('Supplier created successfully.', 'success');
      }

      this.resetForm();
      await this.loadSuppliers();
    } catch (error) {
      console.error('API Error details:', error);
      this.setStatus(error.message || 'An error occurred while saving supplier.', 'error');
    } finally {
      this.submitBtn.disabled = false;
    }
  }

  startEdit(supplier) {
    this.editingId = supplier.id;

    this.nameInput.value = supplier.name || '';
    this.emailInput.value = supplier.contactEmail || '';
    this.phoneInput.value = supplier.contactPhone || '';
    this.addressInput.value = supplier.address || '';
    this.isActiveCheckbox.checked = supplier.isActive !== undefined ? supplier.isActive : true;

    this.formTitle.textContent = 'Edit Supplier';
    this.submitBtn.textContent = 'Update Supplier';
    this.cancelBtn.hidden = false;
    this.nameInput.focus();
  }

  async deleteSupplier(id) {
    if (!confirm('Are you sure you want to delete this supplier?')) return;

    try {
      await supplierService.delete(id);
      removeSupplierFromState(id);
      this.setStatus('Supplier deleted successfully.', 'success');
      this.renderSuppliers();
    } catch (error) {
      this.setStatus('Failed to delete supplier: ' + error.message, 'error');
    }
  }

  resetForm() {
    this.editingId = null;
    this.form.reset();
    this.isActiveCheckbox.checked = true;
    this.formTitle.textContent = 'Add New Supplier';
    this.submitBtn.textContent = 'Save Supplier';
    this.cancelBtn.hidden = true;
  }

  showLoading(isLoading) {
    if (this.suppliersLoading) {
      this.suppliersLoading.hidden = !isLoading;
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
  new SuppliersPage();
});