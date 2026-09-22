import { purchaseOrderService } from '../services/purchaseOrderService.js';
import { productService } from '../services/ProductService.js';
import { supplierService } from '../services/SupplierService.js';
import { warehouseService } from '../services/WarehouseService.js';
import { validatePurchaseOrder, buildPurchaseCreatePayload } from '../validation/purchaseOrderValidation.js';
import {
  purchaseOrderState,
  setOrders,
  setSuppliers,
  setWarehouses,
  setProducts,
  addLineItem,
  updateLineItem,
  removeLineItem,
  clearLineItems,
  calculateGrandTotal
} from '../state/purchaseOrderState.js';

export class PurchaseOrdersPage {
  constructor() {
    this.initElements();
    this.bindEvents();
    this.init();
  }

  initElements() {
    this.form = document.getElementById('purchase-order-form');
    this.supplierSelect = document.getElementById('po-supplier');
    this.warehouseSelect = document.getElementById('po-warehouse');
    this.itemsTbody = document.getElementById('po-items-body');
    this.addItemBtn = document.getElementById('add-po-item-btn');
    this.totalDisplay = document.getElementById('po-total-display');
    this.submitBtn = document.getElementById('submit-btn');
    this.formStatus = document.getElementById('form-status');

    this.ordersList = document.getElementById('purchase-orders-list');
    this.ordersEmpty = document.getElementById('purchase-orders-empty');
    this.ordersLoading = document.getElementById('purchase-orders-loading');
  }

  bindEvents() {
    if (this.addItemBtn) {
      this.addItemBtn.addEventListener('click', () => {
        addLineItem();
        this.renderLineItems();
      });
    }

    if (this.form) {
      this.form.addEventListener('submit', (e) => this.handleSubmit(e));
    }

    if (this.itemsTbody) {
      this.itemsTbody.addEventListener('input', (e) => this.handleLineItemInput(e));
      this.itemsTbody.addEventListener('change', (e) => this.handleLineItemChange(e));
      this.itemsTbody.addEventListener('click', (e) => {
        const removeBtn = e.target.closest('.remove-item-btn');
        if (removeBtn) {
          const id = parseFloat(removeBtn.getAttribute('data-id'));
          removeLineItem(id);
          this.renderLineItems();
        }
      });
    }

    if (this.ordersList) {
      this.ordersList.addEventListener('click', (e) => this.handleListActions(e));
    }
  }

  async init() {
    await Promise.all([
      this.loadDropdownData(),
      this.loadOrders()
    ]);
  }

  async loadDropdownData() {
    try {
      const [suppliersData, warehousesData, productsData] = await Promise.all([
        supplierService.getAll().catch(() => []),
        warehouseService.getAll().catch(() => []),
        productService.getAll().catch(() => [])
      ]);

      const suppliers = Array.isArray(suppliersData) ? suppliersData : (suppliersData.items || []);
      const warehouses = Array.isArray(warehousesData) ? warehousesData : (warehousesData.items || []);
      const products = Array.isArray(productsData) ? productsData : (productsData.items || []);

      setSuppliers(suppliers);
      setWarehouses(warehouses);
      setProducts(products);

      this.populateSelect(this.supplierSelect, purchaseOrderState.suppliers, 'Select Supplier...');
      this.populateSelect(this.warehouseSelect, purchaseOrderState.warehouses, 'Select Destination Warehouse...');
    } catch (err) {
      console.error('Failed to load dropdown options:', err);
    }
  }

  populateSelect(selectElement, items, defaultLabel) {
    if (!selectElement) return;
    selectElement.innerHTML = `<option value="">${defaultLabel}</option>`;
    items.forEach(item => {
      const option = document.createElement('option');
      option.value = item.id;
      option.textContent = item.name || `ID: ${item.id}`;
      selectElement.appendChild(option);
    });
  }

  renderLineItems() {
    this.itemsTbody.innerHTML = '';

    if (purchaseOrderState.lineItems.length === 0) {
      this.itemsTbody.innerHTML = `
        <tr>
          <td colspan="5" class="empty-state">No items added yet.</td>
        </tr>
      `;
      this.updateTotalDisplay();
      return;
    }

    purchaseOrderState.lineItems.forEach(item => {
      const row = document.createElement('tr');
      row.className = 'po-line-item-row';

      let productOptions = '<option value="">Select Product...</option>';
      purchaseOrderState.products.forEach(p => {
        const isSelected = String(p.id) === String(item.productId) ? 'selected' : '';
        productOptions += `<option value="${p.id}" data-cost="${p.unitPrice || p.costPrice || 0}" ${isSelected}>${p.name} (${p.sku || ''})</option>`;
      });

      const qty = parseInt(item.quantityOrdered, 10) || 0;
      const price = parseFloat(item.agreedUnitPrice) || 0;
      const subtotal = qty * price;

      row.innerHTML = `
        <td>
          <select class="form-control po-item-product" data-id="${item.id}">
            ${productOptions}
          </select>
        </td>
        <td>
          <input type="number" class="form-control po-item-qty" data-id="${item.id}" min="1" value="${item.quantityOrdered}" />
        </td>
        <td>
          <input type="number" class="form-control po-item-price" data-id="${item.id}" min="0.01" step="0.01" value="${item.agreedUnitPrice || ''}" placeholder="0.00" />
        </td>
        <td class="po-item-subtotal-cell">$${subtotal.toFixed(2)}</td>
        <td>
          <button type="button" class="btn btn-danger-sm remove-item-btn" data-id="${item.id}" title="Remove Item">&times;</button>
        </td>
      `;

      this.itemsTbody.appendChild(row);
    });

    this.updateTotalDisplay();
  }

  handleLineItemChange(e) {
    const target = e.target;
    if (target.classList.contains('po-item-product')) {
      const id = parseFloat(target.getAttribute('data-id'));
      const productId = target.value;
      const selectedOption = target.options[target.selectedIndex];
      const defaultCost = selectedOption?.getAttribute('data-cost') || 0;

      updateLineItem(id, {
        productId,
        agreedUnitPrice: defaultCost
      });
      this.renderLineItems();
    }
  }

  handleLineItemInput(e) {
    const target = e.target;
    const id = parseFloat(target.getAttribute('data-id'));

    if (target.classList.contains('po-item-qty')) {
      updateLineItem(id, { quantityOrdered: target.value });
      this.updateRowSubtotal(target);
    } else if (target.classList.contains('po-item-price')) {
      updateLineItem(id, { agreedUnitPrice: target.value });
      this.updateRowSubtotal(target);
    }
  }

  updateRowSubtotal(target) {
    const row = target.closest('tr');
    if (!row) return;

    const qtyInput = row.querySelector('.po-item-qty');
    const priceInput = row.querySelector('.po-item-price');
    const subtotalCell = row.querySelector('.po-item-subtotal-cell');

    const qty = parseInt(qtyInput?.value, 10) || 0;
    const price = parseFloat(priceInput?.value) || 0;
    const subtotal = qty * price;

    if (subtotalCell) {
      subtotalCell.textContent = `$${subtotal.toFixed(2)}`;
    }

    this.updateTotalDisplay();
  }

  updateTotalDisplay() {
    if (this.totalDisplay) {
      this.totalDisplay.textContent = `$${calculateGrandTotal().toFixed(2)}`;
    }
  }

  async loadOrders() {
    if (this.ordersLoading) this.ordersLoading.hidden = false;
    if (this.ordersEmpty) this.ordersEmpty.hidden = true;

    try {
      const data = await purchaseOrderService.getAll();
      setOrders(Array.isArray(data) ? data : (data.items || []));
      this.renderOrdersList();
    } catch (err) {
      console.error('Failed to load purchase orders:', err);
      if (this.ordersEmpty) {
        this.ordersEmpty.textContent = 'Failed to load purchase orders.';
        this.ordersEmpty.hidden = false;
      }
    } finally {
      if (this.ordersLoading) this.ordersLoading.hidden = true;
    }
  }

  renderOrdersList() {
    this.ordersList.innerHTML = '';

    if (!purchaseOrderState.orders || purchaseOrderState.orders.length === 0) {
      if (this.ordersEmpty) {
        this.ordersEmpty.textContent = 'No purchase orders yet. Create one above.';
        this.ordersEmpty.hidden = false;
      }
      return;
    }

    if (this.ordersEmpty) this.ordersEmpty.hidden = true;

    purchaseOrderState.orders.forEach(order => {
      const card = document.createElement('div');
      card.className = 'card order-card';

      const statusClass = (order.orderStatus || '').toLowerCase();
      const isPending = statusClass === 'pending' || statusClass === 'created' || statusClass === 'draft';

      let itemsRows = '';
      if (Array.isArray(order.items)) {
        order.items.forEach(item => {
          itemsRows += `
            <tr>
              <td>${item.productName || 'Product #' + item.productId} (${item.sku || '-'})</td>
              <td>${item.quantityOrdered}</td>
              <td>$${parseFloat(item.agreedUnitPrice).toFixed(2)}</td>
              <td>$${parseFloat(item.lineTotal).toFixed(2)}</td>
            </tr>
          `;
        });
      }

      card.innerHTML = `
        <div class="order-header">
          <div>
            <h3>PO #${order.poNumber || order.id}</h3>
            <p class="order-meta">
              Supplier: <strong>${order.supplierName || 'ID: ' + order.supplierId}</strong> |
              Warehouse: <strong>${order.warehouseName || 'ID: ' + order.warehouseId}</strong> |
              Date: ${new Date(order.orderDate).toLocaleDateString()}
            </p>
          </div>
          <div class="order-header-right">
            <span class="badge badge-${statusClass}">${order.orderStatus}</span>
            <span class="order-total-price">$${parseFloat(order.totalCostAmount).toFixed(2)}</span>
          </div>
        </div>

        <div class="data-table-wrapper">
          <table class="data-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Qty Ordered</th>
                <th>Agreed Unit Price</th>
                <th>Line Total</th>
              </tr>
            </thead>
            <tbody>
              ${itemsRows}
            </tbody>
          </table>
        </div>

        ${isPending ? `
          <div class="order-actions">
            <button type="button" class="btn btn-success btn-receive" data-id="${order.id}">Receive Order</button>
            <button type="button" class="btn btn-danger btn-cancel" data-id="${order.id}">Cancel Order</button>
          </div>
        ` : ''}
      `;

      this.ordersList.appendChild(card);
    });
  }

  async handleListActions(e) {
    const receiveBtn = e.target.closest('.btn-receive');
    const cancelBtn = e.target.closest('.btn-cancel');

    if (receiveBtn) {
      const id = receiveBtn.getAttribute('data-id');
      if (confirm(`Are you sure you want to mark Purchase Order #${id} as received? Stock will be updated.`)) {
        try {
          receiveBtn.disabled = true;
          await purchaseOrderService.receiveOrder(id);
          await this.loadOrders();
        } catch (err) {
          alert(err.message || 'Failed to receive order.');
        }
      }
    } else if (cancelBtn) {
      const id = cancelBtn.getAttribute('data-id');
      if (confirm(`Are you sure you want to cancel Purchase Order #${id}?`)) {
        try {
          cancelBtn.disabled = true;
          await purchaseOrderService.cancelOrder(id);
          await this.loadOrders();
        } catch (err) {
          alert(err.message || 'Failed to cancel order.');
        }
      }
    }
  }

  async handleSubmit(e) {
    e.preventDefault();
    this.setStatus('', '');

    const rawFormData = {
      supplierId: this.supplierSelect?.value,
      warehouseId: this.warehouseSelect?.value,
      items: purchaseOrderState.lineItems
    };

    const validation = validatePurchaseOrder(rawFormData);
    if (!validation.isValid) {
      this.setStatus(validation.errors.join(' | '), 'error');
      return;
    }

    const payload = buildPurchaseCreatePayload(rawFormData);

    try {
      if (this.submitBtn) this.submitBtn.disabled = true;
      await purchaseOrderService.create(payload);
      this.setStatus('Purchase order created successfully.', 'success');

      this.form.reset();
      clearLineItems();
      this.renderLineItems();

      await this.loadOrders();
    } catch (err) {
      console.error('Failed to create purchase order:', err);
      this.setStatus(err.message || 'Failed to create purchase order.', 'error');
    } finally {
      if (this.submitBtn) this.submitBtn.disabled = false;
    }
  }

  setStatus(message, type) {
    if (!this.formStatus) return;
    this.formStatus.textContent = message;
    this.formStatus.className = `form-status ${type}`;
  }
}

document.addEventListener('DOMContentLoaded', () => {
  new PurchaseOrdersPage();
});