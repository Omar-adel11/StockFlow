import * as customerService from '../services/CustomerService.js';
import { validateCustomerForm, buildCustomerPayload } from '../validation/customerValidation.js';
import { getAccessToken, clearSession } from '../sessions/session.js';
import {
    setCustomers,
    getCustomers,
    addCustomerToState,
    updateCustomerInState,
    removeCustomerFromState
} from '../state/customerState.js';

class CustomersPage {
    constructor() {
        this.editingId = null;
        this.initElements();
        this.bindEvents();
        this.init();
    }

    initElements() {
        // Sidebar & Auth controls
        this.logoutBtn = document.getElementById('logout-btn');

        // Form structural elements
        this.form = document.getElementById('customer-form');
        this.formTitle = document.getElementById('form-title');
        this.submitBtn = document.getElementById('submit-btn');
        this.cancelBtn = document.getElementById('cancel-btn');
        this.formStatus = document.getElementById('form-status');

        // Form field inputs
        this.nameInput = document.getElementById('customer-name');
        this.emailInput = document.getElementById('customer-email');
        this.phoneInput = document.getElementById('customer-phone');
        this.streetInput = document.getElementById('customer-address-street');
        this.cityInput = document.getElementById('customer-address-city');
        this.stateInput = document.getElementById('customer-address-state');
        this.postalCodeInput = document.getElementById('customer-address-postal-code');
        this.countryInput = document.getElementById('customer-address-country');

        // Data list & feedback states
        this.customersList = document.getElementById('customers-list');
        this.customersEmpty = document.getElementById('customers-empty');
        this.customersLoading = document.getElementById('customers-loading');
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

        await this.loadCustomers();
    }

    async loadCustomers() {
        this.showLoading(true);
        try {
            const customersData = await customerService.getAllCustomers();
            setCustomers(customersData || []);
            this.renderCustomers();
        } catch (error) {
            this.setStatus('Failed to load customers: ' + error.message, 'error');
        } finally {
            this.showLoading(false);
        }
    }

    renderCustomers() {
        const customers = getCustomers();
        this.customersList.innerHTML = '';

        if (!customers || customers.length === 0) {
            this.customersEmpty.hidden = false;
            return;
        }

        this.customersEmpty.hidden = true;

        customers.forEach((customer) => {
            const card = document.createElement('div');
            card.className = 'card entity-card';
            
            // Extract primary/first address from the addresses array
            const addr = (customer.addresses && customer.addresses.length > 0) 
                ? customer.addresses[0] 
                : (customer.address || {});

            const zip = addr.zipCode || addr.postalCode || '';
            const addressString = [addr.street, addr.city, addr.state, zip, addr.country]
                .filter(Boolean)
                .join(', ') || 'No address specified';

            card.innerHTML = `
                <div class="entity-card-header">
                    <h3>${this.escapeHtml(customer.name)}</h3>
                </div>
                <div class="entity-card-body">
                    <p><strong>Email:</strong> ${this.escapeHtml(customer.email)}</p>
                    <p><strong>Phone:</strong> ${this.escapeHtml(customer.phone)}</p>
                    <p><strong>Address:</strong> ${this.escapeHtml(addressString)}</p>
                </div>
                <div class="entity-card-actions">
                    <button type="button" class="btn btn-secondary btn-sm edit-btn">Edit</button>
                    <button type="button" class="btn btn-danger btn-sm delete-btn">Delete</button>
                </div>
            `;

            card.querySelector('.edit-btn').addEventListener('click', () => this.startEdit(customer));
            card.querySelector('.delete-btn').addEventListener('click', () => this.deleteCustomer(customer.id));

            this.customersList.appendChild(card);
        });
    }

    async handleSubmit(event) {
    event.preventDefault();
    this.setStatus('');

    const rawFormData = {
        name: this.nameInput.value,
        email: this.emailInput.value,
        phone: this.phoneInput.value,
        addressStreet: this.streetInput.value,
        addressCity: this.cityInput.value,
        addressState: this.stateInput.value,
        addressPostalCode: this.postalCodeInput.value,
        addressCountry: this.countryInput.value
    };

    const validation = validateCustomerForm(rawFormData);
    if (!validation.isValid) {
        this.setStatus(validation.errors.join(' '), 'error');
        return;
    }

    const payload = buildCustomerPayload(rawFormData);

    try {
        this.submitBtn.disabled = true;

        if (this.editingId) {
            const updatedCustomer = await customerService.updateCustomer(this.editingId, payload);
            updateCustomerInState(updatedCustomer || { id: this.editingId, ...payload });
            this.setStatus('Customer updated successfully.', 'success');
        } else {
            const newCustomer = await customerService.createCustomer(payload);
            addCustomerToState(newCustomer || payload);
            this.setStatus('Customer created successfully.', 'success');
        }

        this.resetForm();
        this.renderCustomers();
    } catch (error) {
        console.error("API Error details:", error);

        // 1. Check ASP.NET Core Validation Errors dictionary: { "Addresses[0].ZipCode": ["The field ZipCode is required."] }
        if (error.data && error.data.errors) {
            const messages = [];
            for (const [field, errList] of Object.entries(error.data.errors)) {
                const cleanField = field.replace(/^addresses\[\d+\]\./i, '').replace(/^addresses\./i, '');
                messages.push(`${cleanField}: ${Array.isArray(errList) ? errList.join(', ') : errList}`);
            }
            this.setStatus(`Validation Failed: ${messages.join(' | ')}`, 'error');
        } 
        // 2. Check for detail string in ProblemDetails
        else if (error.data && error.data.detail) {
            this.setStatus(`Error: ${error.data.detail}`, 'error');
        } 
        // 3. Fallback to error message
        else {
            this.setStatus(error.message || 'Validation Error occurred on server.', 'error');
        }
    } finally {
        this.submitBtn.disabled = false;
    }
}

    startEdit(customer) {
        this.editingId = customer.id;
        
        this.nameInput.value = customer.name || '';
        this.emailInput.value = customer.email || '';
        this.phoneInput.value = customer.phone || '';

        const addr = (customer.addresses && customer.addresses.length > 0) 
            ? customer.addresses[0] 
            : (customer.address || {});

        this.streetInput.value = addr.street || '';
        this.cityInput.value = addr.city || '';
        this.stateInput.value = addr.state || '';
        this.postalCodeInput.value = addr.zipCode || addr.postalCode || '';
        this.countryInput.value = addr.country || '';

        this.formTitle.textContent = 'Edit Customer';
        this.submitBtn.textContent = 'Update Customer';
        this.cancelBtn.hidden = false;
        this.nameInput.focus();
    }

    async deleteCustomer(id) {
        if (!confirm('Are you sure you want to delete this customer?')) return;

        try {
            await customerService.deleteCustomer(id);
            removeCustomerFromState(id);
            this.setStatus('Customer deleted successfully.', 'success');
            this.renderCustomers();
        } catch (error) {
            this.setStatus('Failed to delete customer: ' + error.message, 'error');
        }
    }

    resetForm() {
        this.editingId = null;
        this.form.reset();
        this.formTitle.textContent = 'Add New Customer';
        this.submitBtn.textContent = 'Save Customer';
        this.cancelBtn.hidden = true;
    }

    showLoading(isLoading) {
        if (this.customersLoading) {
            this.customersLoading.hidden = !isLoading;
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
    new CustomersPage();
});