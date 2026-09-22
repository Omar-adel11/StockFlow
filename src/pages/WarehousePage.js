import { warehouseService } from '../services/WarehouseService.js';
import { validateWarehouseForm, buildWarehousePayload } from '../validation/warehouseValidation.js';
import { getAccessToken, clearSession } from '../sessions/session.js';
import {
  setWarehouses,
  getWarehouses,
  addWarehouseToState,
  updateWarehouseInState,
  removeWarehouseFromState
} from '../state/warehouseState.js';

class WarehousesPage {
  constructor() {
    this.editingId = null;
    this.initElements();
    this.bindEvents();
    this.init();
  }

  initElements() {
    this.logoutBtn = document.getElementById('logout-btn');

    this.form = document.getElementById('warehouse-form');
    this.formTitle = document.getElementById('form-title');
    this.submitBtn = document.getElementById('submit-btn');
    this.cancelBtn = document.getElementById('cancel-btn');
    this.formStatus = document.getElementById('form-status');

    this.nameInput = document.getElementById('warehouse-name');
    this.locationInput = document.getElementById('warehouse-location');
    
    this.isActiveGroup = document.getElementById('is-active-group');
    this.isActiveCheckbox = document.getElementById('warehouse-is-active');

    this.warehousesList = document.getElementById('warehouses-list');
    this.warehousesEmpty = document.getElementById('warehouses-empty');
    this.warehousesLoading = document.getElementById('warehouses-loading');
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

    await this.loadWarehouses();
  }

  async loadWarehouses() {
    this.showLoading(true);
    try {
      const warehouses = await warehouseService.getAll();
      setWarehouses(warehouses);
      this.renderWarehouses();
    } catch (error) {
      this.setStatus('Failed to load warehouses: ' + error.message, 'error');
    } finally {
      this.showLoading(false);
    }
  }

  renderWarehouses() {
    const warehouses = getWarehouses();
    this.warehousesList.innerHTML = '';

    if (!warehouses || warehouses.length === 0) {
      this.warehousesEmpty.hidden = false;
      return;
    }

    this.warehousesEmpty.hidden = true;

    warehouses.forEach((warehouse) => {
      // Support both property naming conventions (warehouseName/locationAddress or name/location)
      const name = warehouse.warehouseName || warehouse.name || '';
      const location = warehouse.locationAddress || warehouse.location || '';
      const isActive = warehouse.isActive !== undefined ? warehouse.isActive : true;

      const card = document.createElement('div');
      card.className = 'card entity-card';

      card.innerHTML = `
        <div class="entity-card-header">
          <h3>${this.escapeHtml(name)}</h3>
          <span class="badge ${isActive ? 'badge-success' : 'badge-danger'}">
            ${isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
        <div class="entity-card-body">
          <p><strong>Location:</strong> ${this.escapeHtml(location)}</p>
        </div>
        <div class="entity-card-actions">
          <button type="button" class="btn btn-secondary btn-sm edit-btn">Edit</button>
          <button type="button" class="btn btn-danger btn-sm delete-btn">Delete</button>
        </div>
      `;

      card.querySelector('.edit-btn').addEventListener('click', () => this.startEdit(warehouse));
      card.querySelector('.delete-btn').addEventListener('click', () => this.deleteWarehouse(warehouse.id));

      this.warehousesList.appendChild(card);
    });
  }

  async handleSubmit(event) {
    event.preventDefault();
    this.setStatus('');

    const rawFormData = {
      name: this.nameInput.value,
      location: this.locationInput.value,
      isActive: this.isActiveCheckbox.checked
    };

    const validation = validateWarehouseForm(rawFormData);
    if (!validation.isValid) {
      this.setStatus(validation.errors.join(' '), 'error');
      return;
    }

    const isEdit = Boolean(this.editingId);
    const payload = buildWarehousePayload(rawFormData, isEdit);

    try {
      this.submitBtn.disabled = true;

      if (isEdit) {
        const updatedWarehouse = await warehouseService.update(this.editingId, payload);
        updateWarehouseInState(updatedWarehouse || { id: this.editingId, ...payload });
        this.setStatus('Warehouse updated successfully.', 'success');
      } else {
        const newWarehouse = await warehouseService.create(payload);
        addWarehouseToState(newWarehouse || payload);
        this.setStatus('Warehouse created successfully.', 'success');
      }

      this.resetForm();
      await this.loadWarehouses();
    } catch (error) {
      console.error('API Error details:', error);
      this.setStatus(error.message || 'An error occurred while saving warehouse.', 'error');
    } finally {
      this.submitBtn.disabled = false;
    }
  }

  startEdit(warehouse) {
    this.editingId = warehouse.id;

    // Read properties accounting for API naming
    this.nameInput.value = warehouse.warehouseName || warehouse.name || '';
    this.locationInput.value = warehouse.locationAddress || warehouse.location || '';
    
    this.isActiveCheckbox.checked = warehouse.isActive !== undefined ? warehouse.isActive : true;
    if (this.isActiveGroup) {
      this.isActiveGroup.hidden = false;
    }

    this.formTitle.textContent = 'Edit Warehouse';
    this.submitBtn.textContent = 'Update Warehouse';
    this.cancelBtn.hidden = false;
    this.nameInput.focus();
  }

  async deleteWarehouse(id) {
    if (!confirm('Are you sure you want to delete this warehouse?')) return;

    try {
      await warehouseService.delete(id);
      removeWarehouseFromState(id);
      this.setStatus('Warehouse deleted successfully.', 'success');
      this.renderWarehouses();
    } catch (error) {
      this.setStatus('Failed to delete warehouse: ' + error.message, 'error');
    }
  }

  resetForm() {
    this.editingId = null;
    this.form.reset();
    
    if (this.isActiveGroup) {
      this.isActiveGroup.hidden = true;
    }

    this.formTitle.textContent = 'Add New Warehouse';
    this.submitBtn.textContent = 'Save Warehouse';
    this.cancelBtn.hidden = true;
  }

  showLoading(isLoading) {
    if (this.warehousesLoading) {
      this.warehousesLoading.hidden = !isLoading;
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
  new WarehousesPage();
});