import { warehouseService } from '../services/WarehouseService.js';
import { showConfirm, showNotice } from '../utils/ui.js';

const id = new URLSearchParams(location.search).get('id');
const form = document.getElementById('entity-form');
const status = document.getElementById('form-status');
const nameInput = document.getElementById('warehouse-name');
const locationInput = document.getElementById('warehouse-location');

async function init() {
  if (!id) return;
  const x = await warehouseService.getById(id);
  nameInput.value = x.warehouseName || x.WarehouseName || x.name || '';
  locationInput.value = x.locationAddress || x.LocationAddress || x.location || '';
  document.getElementById('page-title').textContent = 'Edit Warehouse';
  document.getElementById('form-title').textContent = 'Edit Warehouse';
  document.querySelector('#entity-form button[type="submit"]').textContent = 'Update Warehouse';
}

form?.addEventListener('submit', async e => {
  e.preventDefault();
  const WarehouseName = nameInput.value.trim();
  const LocationAddress = locationInput.value.trim();

  if (!WarehouseName || !LocationAddress) {
    status.textContent = 'Name and location are required.';
    return;
  }

  try {
    const payload = id
      ? { WarehouseName, LocationAddress, IsActive: true }
      : { WarehouseName, LocationAddress };

    await (id ? warehouseService.update(id, payload) : warehouseService.create(payload));
    showNotice(id ? 'Warehouse updated successfully.' : 'Warehouse created successfully.');
    setTimeout(() => { location.href = 'warehouses.html'; }, 700);
  } catch (err) {
    status.textContent = err.message || 'Failed to save warehouse.';
  }
});

init().catch(e => { status.textContent = e.message || 'Failed to load warehouse.'; });