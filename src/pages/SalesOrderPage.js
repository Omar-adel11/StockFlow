import { salesOrderService } from '../services/salesOrderService.js';
import { productService } from '../services/ProductService.js';
import { customerService } from '../services/CustomerService.js';
import { warehouseService } from '../services/WarehouseService.js';
import { validateSalesOrder, buildSalesCreatePayload } from '../validation/salesOrderValidation.js';
import {
  salesOrderState,
  setOrders,
  setCustomers,
  setWarehouses,
  setProducts,
  addLineItem,
  updateLineItem,
  removeLineItem,
  clearLineItems,
  calculateGrandTotal
} from '../state/salesOrderState.js';

export class SalesOrdersPage {
  constructor() {
    this.initElements();
    this.bindEvents();
    this.init();
  }

  initElements() {
    this.form = document.getElementById('sales-order-form');
    this.customerSelect = document.getElementById('so-customer');
    this.warehouseSelect = document.getElementById('so-warehouse');
    this.itemsTbody = document.getElementById('so-items-body');
    this.addItemBtn = document.getElementById('add-so-item-btn');
    this.totalDisplay = document.getElementById('so-total-display');
    this.submitBtn = document.getElementById('submit-btn');
    this.formStatus = document.getElementById('form-status');

    this.ordersList = document.getElementById('sales-orders-list');
    this.ordersEmpty = document.getElementById('sales-orders-empty');
    this.ordersLoading = document.getElementById('sales-orders-loading');
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
      const [customersData, warehousesData, productsData] = await Promise.all([
        customerService.getAll().catch(() => []),
        warehouseService.getAll().catch(() => []),
        productService.getAll().catch(() => [])
      ]);

      const customers = Array.isArray(customersData) ? customersData : (customersData.items || []);
      const warehouses = Array.isArray(warehousesData) ? warehousesData : (warehousesData.items || []);
      const products = Array.isArray(productsData) ? productsData : (productsData.items || []);

      setCustomers(customers);
      setWarehouses(warehouses);
      setProducts(products);

      this.populateSelect(this.customerSelect, salesOrderState.customers, 'Select Customer...', (c) => c.name || `${c.firstName || ''} ${c.lastName || ''}`.trim() || `ID: ${c.id}`);
      this.populateSelect(this.warehouseSelect, salesOrderState.warehouses, 'Select Fulfilling Warehouse...', (w) => w.name || `ID: ${w.id}`);
    } catch (err) {
      console.error('Failed to load dropdown options:', err);
    }
  }

  populateSelect(selectElement, items, defaultLabel, labelFormatter) {
    if (!selectElement) return;
    selectElement.innerHTML = `<option value="">${defaultLabel}</option>`;
    items.forEach(item => {
      const option = document.createElement('option');
      option.value = item.id;
      option.textContent = labelFormatter ? labelFormatter(item) : (item.name || `ID: ${item.id}`);
      selectElement.appendChild(option);
    });
  }

  renderLineItems() {
    this.itemsTbody.innerHTML = '';

    if (salesOrderState.lineItems.length === 0) {
      this.itemsTbody.innerHTML = `
        <tr>
          <td colspan="5" class="empty-state">No items added yet.</td>
        </tr>
      `;
      this.updateTotalDisplay();
      return;
    }

    salesOrderState.lineItems.forEach(item => {
      const row = document.createElement('tr');
      row.className = 'so-line-item-row';

      let productOptions = '<option value="">Select Product...</option>';
      salesOrderState.products.forEach(p => {
        const isSelected = String(p.id) === String(item.productId) ? 'selected' : '';
        productOptions += `<option value="${p.id}" data-price="${p.unitPrice || p.price || 0}" ${isSelected}>${p.name} (${p.sku || ''})</option>`;
      });

      const qty = parseInt(item.quantitySold, 10) || 0;
      const price = parseFloat(item.billedUnitPrice) || 0;
      const subtotal = qty * price;

      row.innerHTML = `
        <td>
          <select class="form-control so-item-product" data-id="${item.id}">
            ${productOptions}
          </select>
        </td>
        <td>
          <input type="number" class="form-control so-item-qty" data-id="${item.id}" min="1" value="${item.quantitySold}" />
        </td>
        <td>
          <input type="number" class="form-control so-item-price" data-id="${item.id}" min="0.01" step="0.01" value="${item.billedUnitPrice || ''}" placeholder="0.00" />
        </td>
        <td class="so-item-subtotal-cell">$${subtotal.toFixed(2)}</td>
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
    if (target.classList.contains('so-item-product')) {
      const id = parseFloat(target.getAttribute('data-id'));
      const productId = target.value;
      const selectedOption = target.options[target.selectedIndex];
      const defaultPrice = selectedOption?.getAttribute('data-price') || 0;

      updateLineItem(id, {
        productId,
        billedUnitPrice: defaultPrice
      });
      this.renderLineItems();
    }
  }

  handleLineItemInput(e) {
    const target = e.target;
    const id = parseFloat(target.getAttribute('data-id'));

    if (target.classList.contains('so-item-qty')) {
      updateLineItem(id, { quantitySold: target.value });
      this.updateRowSubtotal(target);
    } else if (target.classList.contains('so-item-price')) {
      updateLineItem(id, { billedUnitPrice: target.value });
      this.updateRowSubtotal(target);
    }
  }

  updateRowSubtotal(target) {
    const row = target.closest('tr');
    if (!row) return;

    const qtyInput = row.querySelector('.so-item-qty');
    const priceInput = row.querySelector('.so-item-price');
    const subtotalCell = row.querySelector('.so-item-subtotal-cell');

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
      const data = await salesOrderService.getAll();
      setOrders(Array.isArray(data) ? data : (data.items || []));
      this.renderOrdersList();
    } catch (err) {
      console.error('Failed to load sales orders:', err);
      if (this.ordersEmpty) {
        this.ordersEmpty.textContent = 'Failed to load sales orders.';
        this.ordersEmpty.hidden = false;
      }
    } finally {
      if (this.ordersLoading) this.ordersLoading.hidden = true;
    }
  }

  renderOrdersList() {
    this.ordersList.innerHTML = '';

    if (!salesOrderState.orders || salesOrderState.orders.length === 0) {
      if (this.ordersEmpty) {
        this.ordersEmpty.textContent = 'No sales orders yet. Create one above.';
        this.ordersEmpty.hidden = false;
      }
      return;
    }

    if (this.ordersEmpty) this.ordersEmpty.hidden = true;

    salesOrderState.orders.forEach(order => {
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
              <td>${item.quantitySold}</td>
              <td>$${parseFloat(item.billedUnitPrice).toFixed(2)}</td>
              <td>$${parseFloat(item.lineTotal).toFixed(2)}</td>
            </tr>
          `;
        });
      }

      card.innerHTML = `
        <div class="order-header">
          <div>
            <h3>SO #${order.invoiceNumber || order.id}</h3>
            <p class="order-meta">
              Customer: <strong>${order.customerName || 'ID: ' + order.customerId}</strong> |
              Warehouse: <strong>${order.warehouseName || 'ID: ' + order.warehouseId}</strong> |
              Date: ${new Date(order.orderDate).toLocaleDateString()}
            </p>
          </div>
          <div class="order-header-right">
            <span class="badge badge-${statusClass}">${order.orderStatus}</span>
            <span class="order-total-price">$${parseFloat(order.totalSaleAmount).toFixed(2)}</span>
          </div>
        </div>

        <div class="data-table-wrapper">
          <table class="data-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Qty Sold</th>
                <th>Billed Unit Price</th>
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
            <button type="button" class="btn btn-success btn-fulfill" data-id="${order.id}">Fulfill Order</button>
            <button type="button" class="btn btn-danger btn-cancel" data-id="${order.id}">Cancel Order</button>
          </div>
        ` : ''}
      `;

      this.ordersList.appendChild(card);
    });
  }

  async handleListActions(e) {
    const fulfillBtn = e.target.closest('.btn-fulfill');
    const cancelBtn = e.target.closest('.btn-cancel');

    if (fulfillBtn) {
      const id = fulfillBtn.getAttribute('data-id');
      if (confirm(`Are you sure you want to fulfill Sales Order #${id}? Stock will be deducted.`)) {
        try {
          fulfillBtn.disabled = true;
          await salesOrderService.fulfillOrder(id);
          await this.loadOrders();
        } catch (err) {
          alert(err.message || 'Failed to fulfill order.');
        }
      }
    } else if (cancelBtn) {
      const id = cancelBtn.getAttribute('data-id');
      if (confirm(`Are you sure you want to cancel Sales Order #${id}?`)) {
        try {
          cancelBtn.disabled = true;
          await salesOrderService.cancelOrder(id);
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
      customerId: this.customerSelect?.value,
      warehouseId: this.warehouseSelect?.value,
      items: salesOrderState.lineItems
    };

    const validation = validateSalesOrder(rawFormData);
    if (!validation.isValid) {
      this.setStatus(validation.errors.join(' | '), 'error');
      return;
    }

    const payload = buildSalesCreatePayload(rawFormData);

    try {
      if (this.submitBtn) this.submitBtn.disabled = true;
      await salesOrderService.create(payload);
      this.setStatus('Sales order created successfully.', 'success');

      this.form.reset();
      clearLineItems();
      this.renderLineItems();

      await this.loadOrders();
    } catch (err) {
      console.error('Failed to create sales order:', err);
      this.setStatus(err.message || 'Failed to create sales order.', 'error');
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
  new SalesOrdersPage();
});