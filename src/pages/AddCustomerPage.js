import { getCustomerById, createCustomer, updateCustomer } from '../services/CustomerService.js';
import { showConfirm, showNotice } from '../utils/ui.js';

const id = new URLSearchParams(location.search).get('id');
const form = document.getElementById('entity-form');
const status = document.getElementById('form-status');

let existingAddressId = null;

function val(id) { return document.getElementById(id)?.value?.trim() || ''; }

function fill(x) {
  const addresses = x.addresses || x.Addresses || [];
  const a = addresses.find(a => a.isDefault ?? a.IsDefault) || addresses[0] || {};
  existingAddressId = a.id || a.Id || null;

  document.getElementById('customer-name').value = x.name || x.Name || '';
  document.getElementById('customer-email').value = x.email || x.Email || '';
  document.getElementById('customer-phone').value = x.phone || x.Phone || '';
  document.getElementById('customer-address-street').value = a.street || a.Street || '';
  document.getElementById('customer-address-city').value = a.city || a.City || '';
  document.getElementById('customer-address-state').value = a.state || a.State || '';
  document.getElementById('customer-address-postal-code').value = a.zipCode || a.ZipCode || '';
  document.getElementById('customer-address-country').value = a.country || a.Country || 'Egypt';

  document.getElementById('page-title').textContent = 'Edit Customer';
  document.getElementById('form-title').textContent = 'Edit Customer';
  form.querySelector('button[type="submit"]').textContent = 'Update Customer';
}

async function init() {
  if (id) fill(await getCustomerById(id));
}

form?.addEventListener('submit', async e => {
  e.preventDefault();

  const Name = val('customer-name');
  const Email = val('customer-email');
  const Phone = val('customer-phone');

  if (!Name || !Email || !Phone) {
    status.textContent = 'Name, email and phone are required.';
    return;
  }

  const address = {
    Id: existingAddressId,
    Street: val('customer-address-street'),
    City: val('customer-address-city'),
    State: val('customer-address-state') || 'NA',
    ZipCode: val('customer-address-postal-code'),
    Country: val('customer-address-country') || 'Egypt',
    IsDefault: true
  };

  const payload = { Name, Email, Phone, Addresses: [address] };

  try {
    const confirmed = id
      ? await showConfirm('Update this customer?', { confirmText: 'Update', danger: false })
      : true;

    if (!confirmed) return;

    await (id ? updateCustomer(id, payload) : createCustomer(payload));
    showNotice(id ? 'Customer updated successfully.' : 'Customer created successfully.');
    setTimeout(() => { location.href = 'customers.html'; }, 700);
  } catch (err) {
    status.textContent = err.message || 'Failed to save customer.';
  }
});

init().catch(e => { status.textContent = e.message || 'Failed to load customer.'; });