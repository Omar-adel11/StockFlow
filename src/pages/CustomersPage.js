import '../sessions/authGuard.js';
import { showConfirm, showNotice } from '../utils/ui.js';
import * as customerService from '../services/CustomerService.js';
import { getAccessToken } from '../sessions/session.js';
import { logout } from '../services/authService.js';
import { setCustomers, getCustomers, removeCustomerFromState } from '../state/customerState.js';

class CustomersPage {
  constructor() {
    this.customersList = document.getElementById('customers-list');
    this.customersEmpty = document.getElementById('customers-empty');
    this.customersLoading = document.getElementById('customers-loading');
    this.searchInput = document.getElementById('customers-search');
    this.searchTimer = null;
    this.activeCustomer = null;
    this.editingAddressId = null;
    this.injectAddressStyles();
    this.bindEvents();
    this.init();
  }

  bindEvents() {
    document.getElementById('logout-btn')?.addEventListener('click', () => { void logout(); });
    this.searchInput?.addEventListener('input', () => {
      clearTimeout(this.searchTimer);
      this.searchTimer = setTimeout(() => this.loadCustomers(this.searchInput.value || null), 300);
    });
  }

  async init() {
    if (!getAccessToken()) {
      window.location.href = 'login.html';
      return;
    }
    await this.loadCustomers();
  }

  async loadCustomers(search = null) {
    if (this.customersLoading) this.customersLoading.hidden = false;
    try {
      const data = await customerService.getAllCustomers(search);
      setCustomers(data || []);
      this.renderCustomers();
    } catch (error) {
      showNotice('Failed to load customers: ' + (error.message || 'Unknown error'), 'error');
    } finally {
      if (this.customersLoading) this.customersLoading.hidden = true;
    }
  }

  renderCustomers() {
    const customers = getCustomers() || [];
    if (!this.customersList) return;
    this.customersList.innerHTML = '';
    if (this.customersEmpty) this.customersEmpty.hidden = customers.length > 0;
    customers.forEach(customer => {
      const id = customer.id ?? customer.Id;
      const name = customer.name ?? customer.Name ?? '';
      const email = customer.email ?? customer.Email ?? '';
      const phone = customer.phone ?? customer.Phone ?? '';
      const card = document.createElement('article');
      card.className = 'card entity-card';
      card.innerHTML = `
        <div class="entity-card-header"><h3>${this.escapeHtml(name)}</h3></div>
        <div class="entity-card-body">
          <p><strong>Email:</strong> ${this.escapeHtml(email)}</p>
          <p><strong>Phone:</strong> ${this.escapeHtml(phone)}</p>
        </div>
        <div class="entity-card-actions">
          <button type="button" class="btn btn-secondary btn-sm addresses-btn">Addresses</button>
          <button type="button" class="btn btn-secondary btn-sm edit-btn">Edit</button>
          <button type="button" class="btn btn-danger btn-sm delete-btn">Delete</button>
        </div>`;
      card.querySelector('.addresses-btn').addEventListener('click', () => this.openAddresses(id));
      card.querySelector('.edit-btn').addEventListener('click', () => { window.location.href = `addCustomer.html?id=${encodeURIComponent(id)}`; });
      card.querySelector('.delete-btn').addEventListener('click', () => this.deleteCustomer(id));
      this.customersList.appendChild(card);
    });
  }

  async openAddresses(customerId) {
    try {
      this.activeCustomer = await customerService.getCustomerById(customerId);
      this.ensureAddressModal();
      this.renderAddressList();
      this.addressModal.hidden = false;
      document.body.classList.add('address-modal-open');
    } catch (error) {
      showNotice('Failed to load customer addresses: ' + (error.message || 'Unknown error'), 'error');
    }
  }

  getAddresses() {
    return this.activeCustomer?.addresses || this.activeCustomer?.Addresses || [];
  }

  ensureAddressModal() {
    if (this.addressModal) return;
    const overlay = document.createElement('div');
    overlay.className = 'address-modal-overlay';
    overlay.hidden = true;
    overlay.innerHTML = `
      <section class="address-modal" role="dialog" aria-modal="true" aria-labelledby="address-modal-title">
        <header class="address-modal-header">
          <div><p class="address-modal-eyebrow">CUSTOMER DETAILS</p><h2 id="address-modal-title">Customer addresses</h2><p class="address-modal-customer"></p></div>
          <button type="button" class="address-modal-close" aria-label="Close addresses">&times;</button>
        </header>
        <div class="address-modal-content">
          <div class="address-list"></div>
          <section class="address-editor" hidden>
            <h3 class="address-editor-title">Add address</h3>
            <form class="address-form">
              <div class="address-form-grid">
                <label>Street<input name="street" required maxlength="200" /></label>
                <label>City<input name="city" required maxlength="100" /></label>
                <label>State / Region<input name="state" required maxlength="100" /></label>
                <label>Postal code<input name="zipCode" required maxlength="30" /></label>
                <label class="address-country-field">Country<input name="country" required maxlength="100" value="Egypt" /></label>
              </div>
              <label class="address-default-check"><input type="checkbox" name="isDefault" /> Set as default address</label>
              <div class="address-editor-actions">
                <button type="submit" class="btn btn-primary address-save-btn">Save address</button>
                <button type="button" class="btn btn-secondary address-cancel-btn">Cancel</button>
              </div>
            </form>
          </section>
        </div>
        <footer class="address-modal-footer">
          <button type="button" class="btn btn-secondary address-done-btn">Done</button>
          <button type="button" class="btn btn-primary address-add-btn">+ Add address</button>
        </footer>
      </section>`;
    document.body.appendChild(overlay);
    this.addressModal = overlay;
    this.addressList = overlay.querySelector('.address-list');
    this.addressEditor = overlay.querySelector('.address-editor');
    this.addressForm = overlay.querySelector('.address-form');

    overlay.querySelector('.address-modal-close').addEventListener('click', () => this.closeAddresses());
    overlay.querySelector('.address-done-btn').addEventListener('click', () => this.closeAddresses());
    overlay.addEventListener('click', event => { if (event.target === overlay) this.closeAddresses(); });
    document.addEventListener('keydown', event => {
      if (event.key === 'Escape' && this.addressModal && !this.addressModal.hidden) this.closeAddresses();
    });
    overlay.querySelector('.address-add-btn').addEventListener('click', () => this.showAddressEditor());
    overlay.querySelector('.address-cancel-btn').addEventListener('click', () => this.hideAddressEditor());
    this.addressForm.addEventListener('submit', event => this.saveAddress(event));
  }

  renderAddressList() {
    const customerName = this.activeCustomer?.name ?? this.activeCustomer?.Name ?? 'Customer';
    this.addressModal.querySelector('.address-modal-customer').textContent = customerName;
    const addresses = this.getAddresses();
    this.addressList.innerHTML = '';
    if (!addresses.length) {
      this.addressList.innerHTML = '<div class="address-empty"><span class="address-empty-icon">⌖</span><h3>No addresses yet</h3><p>Add an address for this customer to get started.</p></div>';
    } else {
      addresses.forEach(address => {
        const id = address.id ?? address.Id;
        const street = address.street ?? address.Street ?? '';
        const city = address.city ?? address.City ?? '';
        const state = address.state ?? address.State ?? '';
        const zip = address.zipCode ?? address.ZipCode ?? '';
        const country = address.country ?? address.Country ?? '';
        const isDefault = Boolean(address.isDefault ?? address.IsDefault);
        const card = document.createElement('article');
        card.className = 'address-item';
        card.innerHTML = `
          <div class="address-item-main">
            <div class="address-item-title"><h3>${this.escapeHtml(street || 'Address')}</h3>${isDefault ? '<span class="address-default-badge">Default</span>' : ''}</div>
            <p>${this.escapeHtml([city, state, zip, country].filter(Boolean).join(', '))}</p>
          </div>
          <div class="address-item-actions">
            <button type="button" class="btn btn-secondary btn-sm address-edit-btn">Edit</button>
            <button type="button" class="btn btn-danger btn-sm address-delete-btn">Delete</button>
          </div>`;
        card.querySelector('.address-edit-btn').addEventListener('click', () => this.showAddressEditor(address));
        card.querySelector('.address-delete-btn').addEventListener('click', () => this.deleteAddress(id));
        this.addressList.appendChild(card);
      });
    }
    this.addressModal.querySelector('.address-add-btn').hidden = !this.addressEditor.hidden;
  }

  showAddressEditor(address = null) {
    this.editingAddressId = address ? (address.id ?? address.Id) : null;
    this.addressForm.reset();
    const form = this.addressForm;
    form.elements.street.value = address?.street ?? address?.Street ?? '';
    form.elements.city.value = address?.city ?? address?.City ?? '';
    form.elements.state.value = address?.state ?? address?.State ?? '';
    form.elements.zipCode.value = address?.zipCode ?? address?.ZipCode ?? '';
    form.elements.country.value = address?.country ?? address?.Country ?? 'Egypt';
    form.elements.isDefault.checked = Boolean(address?.isDefault ?? address?.IsDefault) || this.getAddresses().length === 0;
    this.addressEditor.querySelector('.address-editor-title').textContent = address ? 'Edit address' : 'Add address';
    this.addressEditor.querySelector('.address-save-btn').textContent = address ? 'Save changes' : 'Save address';
    this.addressEditor.hidden = false;
    this.addressModal.querySelector('.address-add-btn').hidden = true;
    form.elements.street.focus();
  }

  hideAddressEditor() {
    this.addressEditor.hidden = true;
    this.addressForm.reset();
    this.editingAddressId = null;
    this.addressModal.querySelector('.address-add-btn').hidden = false;
  }

  async saveAddress(event) {
    event.preventDefault();
    const form = this.addressForm;
    const payload = {
      Street: form.elements.street.value.trim(),
      City: form.elements.city.value.trim(),
      State: form.elements.state.value.trim(),
      ZipCode: form.elements.zipCode.value.trim(),
      Country: form.elements.country.value.trim(),
      IsDefault: form.elements.isDefault.checked
    };
    if (!payload.Street || !payload.City || !payload.State || !payload.ZipCode || !payload.Country) {
      showNotice('Please complete all address fields.', 'error');
      return;
    }
    const saveButton = this.addressEditor.querySelector('.address-save-btn');
    saveButton.disabled = true;
    try {
      const customerId = this.activeCustomer.id ?? this.activeCustomer.Id;
      if (this.editingAddressId != null) {
        await customerService.updateCustomerAddress(customerId, this.editingAddressId, payload);
        showNotice('Address updated successfully.');
      } else {
        await customerService.addCustomerAddress(customerId, payload);
        showNotice('Address added successfully.');
      }
      this.activeCustomer = await customerService.getCustomerById(customerId);
      this.hideAddressEditor();
      this.renderAddressList();
      await this.loadCustomers(this.searchInput?.value || null);
    } catch (error) {
      showNotice('Failed to save address: ' + (error.message || 'Unknown error'), 'error');
    } finally {
      saveButton.disabled = false;
    }
  }

  async deleteAddress(addressId) {
    if (!await showConfirm('Delete this address? This cannot be undone.')) return;
    try {
      const customerId = this.activeCustomer.id ?? this.activeCustomer.Id;
      await customerService.deleteCustomerAddress(customerId, addressId);
      this.activeCustomer = await customerService.getCustomerById(customerId);
      this.renderAddressList();
      await this.loadCustomers(this.searchInput?.value || null);
      showNotice('Address deleted successfully.');
    } catch (error) {
      showNotice('Failed to delete address: ' + (error.message || 'Unknown error'), 'error');
    }
  }

  async deleteCustomer(id) {
    if (!await showConfirm('Are you sure you want to delete this customer?')) return;
    try {
      await customerService.deleteCustomer(id);
      removeCustomerFromState(id);
      showNotice('Customer deleted successfully.');
      this.renderCustomers();
    } catch (error) {
      showNotice('Failed to delete customer: ' + (error.message || 'Unknown error'), 'error');
    }
  }

  closeAddresses() {
    if (this.addressModal) this.addressModal.hidden = true;
    this.hideAddressEditorIfPresent();
    document.body.classList.remove('address-modal-open');
  }

  hideAddressEditorIfPresent() {
    if (this.addressEditor) this.hideAddressEditor();
  }

  injectAddressStyles() {
    if (document.getElementById('customer-address-management-styles')) return;
    const style = document.createElement('style');
    style.id = 'customer-address-management-styles';
    style.textContent = `
      .address-modal-overlay{position:fixed;inset:0;z-index:10000;background:rgba(15,23,42,.58);display:flex;align-items:center;justify-content:center;padding:24px;backdrop-filter:blur(3px)}
      .address-modal-overlay[hidden],.address-editor[hidden],.address-modal-footer [hidden]{display:none!important}
      .address-modal{width:min(780px,100%);max-height:min(88vh,900px);overflow:auto;background:var(--color-surface,#fff);color:var(--color-text,#172033);border-radius:20px;box-shadow:0 24px 80px rgba(0,0,0,.25)}
      .address-modal-header{display:flex;align-items:flex-start;justify-content:space-between;gap:18px;padding:25px 28px 20px;border-bottom:1px solid var(--color-border,#e5e7eb)}
      .address-modal-header h2{font-size:1.45rem;margin:4px 0 5px}.address-modal-customer{margin:0;color:var(--color-text-muted,#64748b)}
      .address-modal-eyebrow{font-size:.7rem;letter-spacing:.12em;font-weight:800;color:var(--color-primary,#4f46e5);margin:0}
      .address-modal-close{border:0;background:transparent;font-size:2rem;line-height:1;cursor:pointer;color:inherit;padding:0 4px}
      .address-modal-content{padding:22px 28px}.address-list{display:grid;gap:12px}
      .address-item{display:flex;align-items:center;justify-content:space-between;gap:18px;border:1px solid var(--color-border,#e5e7eb);border-radius:14px;padding:16px 18px;background:var(--color-background,#fff)}
      .address-item-main{min-width:0}.address-item-title{display:flex;align-items:center;flex-wrap:wrap;gap:10px}.address-item h3{font-size:1rem;margin:0 0 6px}.address-item p{color:var(--color-text-muted,#64748b);font-size:.9rem;margin:0;overflow-wrap:anywhere}
      .address-default-badge{font-size:.7rem;font-weight:700;color:#166534;background:#dcfce7;border-radius:999px;padding:4px 8px}
      .address-item-actions{display:flex;gap:8px;flex-shrink:0}.address-empty{text-align:center;padding:30px 12px;color:var(--color-text-muted,#64748b)}.address-empty h3{color:var(--color-text,#172033);margin:10px 0 5px}.address-empty p{margin:0}.address-empty-icon{font-size:2rem}
      .address-editor{margin-top:20px;border:1px solid var(--color-border,#e5e7eb);border-radius:14px;padding:20px}.address-editor h3{margin:0 0 16px}
      .address-form-grid{display:grid;grid-template-columns:1fr 1fr;gap:14px}.address-form-grid label{display:grid;gap:7px;font-size:.85rem;font-weight:600}
      .address-form-grid input{width:100%;min-width:0;border:1px solid var(--color-border,#cbd5e1);border-radius:9px;padding:11px 12px;background:var(--color-background,#fff);color:inherit;font:inherit}
      .address-country-field{grid-column:1/-1}.address-default-check{display:flex;align-items:center;gap:9px;margin:16px 0;font-size:.9rem}.address-default-check input{width:auto}
      .address-editor-actions,.address-modal-footer{display:flex;align-items:center;justify-content:flex-end;gap:10px}.address-modal-footer{padding:16px 28px 22px;border-top:1px solid var(--color-border,#e5e7eb)}
      @media(max-width:600px){.address-modal-overlay{padding:10px}.address-modal-header,.address-modal-content{padding:18px}.address-modal-footer{padding:14px 18px 18px}.address-item{align-items:flex-start;flex-direction:column}.address-form-grid{grid-template-columns:1fr}.address-country-field{grid-column:auto}.address-item-actions{width:100%}}
    `;
    document.head.appendChild(style);
  }

  escapeHtml(value) {
    return String(value ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
  }
}

document.addEventListener('DOMContentLoaded', () => new CustomersPage());
