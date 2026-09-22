import { inventoryService } from '../services/InventoryService.js';
import { productService } from '../services/ProductService.js';
import { warehouseService } from '../services/WarehouseService.js';
import { validateStockAdjustment, buildAdjustmentPayload } from '../validation/inventoryValidation.js';

export class InventoryPage {
  constructor() {
    this.inventory = [];
    this.movements = [];
    this.products = [];
    this.warehouses = [];

    this.initElements();
    this.bindEvents();
    this.init();
  }

  initElements() {
    // Filter controls
    this.filterProduct = document.getElementById('filter-product');
    this.filterWarehouse = document.getElementById('filter-warehouse');
    this.filterLowStock = document.getElementById('filter-low-stock-only');

    // Adjust Stock Form controls
    this.adjustForm = document.getElementById('adjust-stock-form');
    this.adjustProduct = document.getElementById('adjust-product');
    this.adjustWarehouse = document.getElementById('adjust-warehouse');
    this.adjustQuantity = document.getElementById('adjust-quantity');
    this.adjustReason = document.getElementById('adjust-reason');
    this.adjustSubmitBtn = document.getElementById('adjust-submit-btn');
    this.adjustStatus = document.getElementById('adjust-form-status');

    // Current Stock Table
    this.inventoryTableBody = document.getElementById('inventory-table-body');
    this.inventoryEmpty = document.getElementById('inventory-empty');
    this.inventoryLoading = document.getElementById('inventory-loading');

    // Recent Movements Table
    this.movementsTableBody = document.getElementById('movements-table-body');
    this.movementsEmpty = document.getElementById('movements-empty');
    this.movementsLoading = document.getElementById('movements-loading');
  }

  bindEvents() {
    if (this.filterProduct) {
      this.filterProduct.addEventListener('change', () => this.loadInventoryData());
    }
    if (this.filterWarehouse) {
      this.filterWarehouse.addEventListener('change', () => this.loadInventoryData());
    }
    if (this.filterLowStock) {
      this.filterLowStock.addEventListener('change', () => this.loadInventoryData());
    }
    if (this.adjustForm) {
      this.adjustForm.addEventListener('submit', (e) => this.handleAdjustSubmit(e));
    }
  }

  async init() {
    await Promise.all([
      this.loadDropdownData(),
      this.loadInventoryData(),
      this.loadMovementsData()
    ]);
  }

  async loadDropdownData() {
    try {
      const [productsData, warehousesData] = await Promise.all([
        productService.getAll().catch(() => []),
        warehouseService.getAll().catch(() => [])
      ]);

      this.products = Array.isArray(productsData) ? productsData : (productsData.items || []);
      this.warehouses = Array.isArray(warehousesData) ? warehousesData : (warehousesData.items || []);

      this.populateSelect(this.filterProduct, this.products, 'All Products');
      this.populateSelect(this.adjustProduct, this.products, 'Select Product...');

      this.populateSelect(this.filterWarehouse, this.warehouses, 'All Warehouses');
      this.populateSelect(this.adjustWarehouse, this.warehouses, 'Select Warehouse...');
    } catch (err) {
      console.error('Failed to populate dropdown options:', err);
    }
  }

  populateSelect(selectElement, items, defaultLabel) {
    if (!selectElement) return;
    selectElement.innerHTML = `<option value="">${defaultLabel}</option>`;

    items.forEach(item => {
      const option = document.createElement('option');
      option.value = item.id;
      option.textContent = item.name || item.productName || `ID: ${item.id}`;
      selectElement.appendChild(option);
    });
  }

  async loadInventoryData() {
    if (this.inventoryLoading) this.inventoryLoading.hidden = false;
    if (this.inventoryEmpty) this.inventoryEmpty.hidden = true;

    try {
      const productId = this.filterProduct?.value;
      const warehouseId = this.filterWarehouse?.value;
      const isLowStock = this.filterLowStock?.checked;

      let data;
      if (isLowStock) {
        data = await inventoryService.getLowStock();
      } else if (productId) {
        data = await inventoryService.getByProduct(productId);
      } else if (warehouseId) {
        data = await inventoryService.getByWarehouse(warehouseId);
      } else {
        data = await inventoryService.getAll();
      }

      this.inventory = Array.isArray(data) ? data : (data.items || []);
      this.renderInventoryTable();
    } catch (error) {
      console.error('Failed to fetch inventory:', error);
      this.inventoryTableBody.innerHTML = '';
      if (this.inventoryEmpty) {
        this.inventoryEmpty.textContent = 'Failed to load inventory levels.';
        this.inventoryEmpty.hidden = false;
      }
    } finally {
      if (this.inventoryLoading) this.inventoryLoading.hidden = true;
    }
  }

  renderInventoryTable() {
    this.inventoryTableBody.innerHTML = '';

    if (!this.inventory || this.inventory.length === 0) {
      if (this.inventoryEmpty) {
        this.inventoryEmpty.textContent = 'No stock records match these filters.';
        this.inventoryEmpty.hidden = false;
      }
      return;
    }

    if (this.inventoryEmpty) this.inventoryEmpty.hidden = true;

    this.inventory.forEach(item => {
      const row = document.createElement('tr');
      const isLow = item.quantityOnHand <= item.reorderLevel;
      const statusBadge = isLow
        ? `<span class="badge badge-warning">Low Stock</span>`
        : `<span class="badge badge-success">In Stock</span>`;

      row.innerHTML = `
        <td>${item.productName || item.product?.name || '-'}</td>
        <td>${item.sku || item.product?.sku || '-'}</td>
        <td>${item.warehouseName || item.warehouse?.name || '-'}</td>
        <td><strong>${item.quantityOnHand ?? 0}</strong></td>
        <td>${item.reorderLevel ?? '-'}</td>
        <td>${statusBadge}</td>
      `;
      this.inventoryTableBody.appendChild(row);
    });
  }

  async loadMovementsData() {
    if (this.movementsLoading) this.movementsLoading.hidden = false;
    if (this.movementsEmpty) this.movementsEmpty.hidden = true;

    try {
      const data = await inventoryService.getStockMovements(20);
      this.movements = Array.isArray(data) ? data : (data.items || []);
      this.renderMovementsTable();
    } catch (error) {
      console.error('Failed to load stock movements:', error);
      this.movementsTableBody.innerHTML = '';
      if (this.movementsEmpty) {
        this.movementsEmpty.textContent = 'Failed to load stock movements.';
        this.movementsEmpty.hidden = false;
      }
    } finally {
      if (this.movementsLoading) this.movementsLoading.hidden = true;
    }
  }

  renderMovementsTable() {
    this.movementsTableBody.innerHTML = '';

    if (!this.movements || this.movements.length === 0) {
      if (this.movementsEmpty) {
        this.movementsEmpty.textContent = 'No stock movements yet.';
        this.movementsEmpty.hidden = false;
      }
      return;
    }

    if (this.movementsEmpty) this.movementsEmpty.hidden = true;

    this.movements.forEach(m => {
      const rawDate = m.timestampUtc || m.createdOn || m.date;
      const dateStr = rawDate ? new Date(rawDate).toLocaleString() : '-';

      const qty = m.quantityChanged ?? m.quantityChange ?? 0;
      const changeClass = qty > 0 ? 'text-success' : (qty < 0 ? 'text-danger' : '');
      const changeSign = qty > 0 ? `+${qty}` : qty;

      const reason = m.movementReason || m.reason || 'Adjustment';

      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${dateStr}</td>
        <td>${m.productName || m.product?.name || '-'}</td>
        <td>${m.warehouseName || m.warehouse?.name || '-'}</td>
        <td class="${changeClass}"><strong>${changeSign}</strong></td>
        <td>${reason}</td>
        <td>${m.referenceId ?? m.reference ?? '-'}</td>
      `;
      this.movementsTableBody.appendChild(row);
    });
  }

  async handleAdjustSubmit(event) {
    event.preventDefault();
    this.setStatus('', '');

    const formData = {
      productId: this.adjustProduct.value,
      warehouseId: this.adjustWarehouse.value,
      quantityChanged: this.adjustQuantity.value,
      reason: this.adjustReason.value
    };

    const validation = validateStockAdjustment(formData);
    if (!validation.isValid) {
      this.setStatus(validation.errors.join(' | '), 'error');
      return;
    }

    const payload = buildAdjustmentPayload(formData);

    try {
      this.adjustSubmitBtn.disabled = true;
      await inventoryService.adjustStock(payload);
      this.setStatus('Stock adjustment recorded successfully.', 'success');
      this.adjustForm.reset();

      // Refresh inventory levels and movements table
      await Promise.all([this.loadInventoryData(), this.loadMovementsData()]);
    } catch (error) {
      console.error('Failed to record stock adjustment:', error);
      this.setStatus(error.message || 'Failed to record stock adjustment.', 'error');
    } finally {
      this.adjustSubmitBtn.disabled = false;
    }
  }

  setStatus(message, type) {
    if (!this.adjustStatus) return;
    this.adjustStatus.textContent = message;
    this.adjustStatus.className = `form-status ${type}`;
  }
}

document.addEventListener('DOMContentLoaded', () => {
  new InventoryPage();
});