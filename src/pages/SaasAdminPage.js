import { getBusinessOwners, updateBusinessOwnerStatus } from '../services/saasAdminService.js';
import { fetchPlans } from '../services/planService.js';
import { showConfirm, showNotice } from '../utils/ui.js';
import * as authService from '../services/authService.js';

const ownerSearch = document.getElementById('owner-search');
const ownersList = document.getElementById('business-owners-list');
const ownersEmpty = document.getElementById('business-owners-empty');
const ownersLoading = document.getElementById('business-owners-loading');
const plansList = document.getElementById('plans-list');
const plansEmpty = document.getElementById('plans-empty');
const plansLoading = document.getElementById('plans-loading');
const logoutBtn = document.getElementById('logout-btn');

let searchTimer;

document.addEventListener('DOMContentLoaded', init);

async function init() {
  logoutBtn?.addEventListener('click', () => authService.logout());
  ownerSearch?.addEventListener('input', () => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => loadOwners(ownerSearch.value), 300);
  });
  await Promise.all([loadOwners(), loadPlans()]);
}

async function loadOwners(search = null) {
  ownersLoading.hidden = false;
  ownersEmpty.hidden = true;
  try {
    const owners = await getBusinessOwners(search);
    renderOwners(Array.isArray(owners) ? owners : []);
  } catch (error) {
    ownersList.innerHTML = '';
    ownersEmpty.textContent = error.message || 'Failed to load business owners.';
    ownersEmpty.hidden = false;
  } finally {
    ownersLoading.hidden = true;
  }
}

function renderOwners(owners) {
  ownersList.innerHTML = '';
  if (!owners.length) {
    ownersEmpty.hidden = false;
    return;
  }
  ownersEmpty.hidden = true;

  owners.forEach(owner => {
    const row = document.createElement('tr');
    row.innerHTML = `
      <td><div class="person-cell"><span class="avatar">${initials(owner.fullName || owner.name)}</span><div><strong>${escapeHtml(owner.fullName || owner.name || '')}</strong><span>${escapeHtml(owner.email || '')}</span></div></div></td>
      <td>${escapeHtml(owner.businessName || '—')}</td>
      <td><span class="plan-badge ${owner.currentPlanName ? '' : 'plan-badge-muted'}">${escapeHtml(owner.currentPlanName || 'No Plan')}</span></td>
      <td>
        <label class="switch">
          <input type="checkbox" ${owner.isActive ? 'checked' : ''} data-owner-id="${owner.id}">
          <span class="switch-slider"></span>
          <span class="switch-label">${owner.isActive ? 'Active' : 'Inactive'}</span>
        </label>
      </td>
      <td class="table-actions"><button type="button" class="btn btn-secondary btn-small assign-plan-btn" data-owner-id="${owner.id}">Assign Plan</button></td>
    `;

    const toggle = row.querySelector('input[type="checkbox"]');
    toggle.addEventListener('change', async () => {
      const next = toggle.checked;
      const confirmed = await showConfirm(`Change ${owner.fullName || owner.name}'s status to ${next ? 'Active' : 'Inactive'}?`, { confirmText: 'Update Status' });
      if (!confirmed) {
        toggle.checked = !next;
        return;
      }
      try {
        toggle.disabled = true;
        await updateBusinessOwnerStatus(owner.id, next);
        showNotice('Business owner status updated successfully.');
        renderOwners(await getBusinessOwners(ownerSearch?.value || null));
      } catch (error) {
        toggle.checked = !next;
        showNotice(error.message || 'Failed to update status.', 'error');
      } finally {
        toggle.disabled = false;
      }
    });

    row.querySelector('.assign-plan-btn')?.addEventListener('click', () => showNotice('Plan assignment UI is ready for the existing modal flow.', 'warning'));

    ownersList.appendChild(row);
  });
}

async function loadPlans() {
  plansLoading.hidden = false;
  plansEmpty.hidden = true;
  try {
    const plans = await fetchPlans();
    renderPlans(Array.isArray(plans) ? plans : []);
  } catch {
    plansList.innerHTML = '';
    plansEmpty.hidden = false;
  } finally {
    plansLoading.hidden = true;
  }
}

function renderPlans(plans) {
  plansList.innerHTML = '';
  if (!plans.length) {
    plansEmpty.hidden = false;
    return;
  }
  plansEmpty.hidden = true;
  plans.forEach(plan => {
    const card = document.createElement('article');
    card.className = 'card plan-card';
    card.innerHTML = `
      <div class="plan-card-header"><div><h3>${escapeHtml(plan.name)}</h3></div><span class="badge ${plan.isActive ? 'badge-active' : 'badge-inactive'}">${plan.isActive ? 'Active' : 'Inactive'}</span></div>
      <p class="plan-price">$${Number(plan.price || 0).toFixed(2)} <span>/ ${escapeHtml(plan.billingCycle || 'Monthly')}</span></p>
      <p class="plan-description">${escapeHtml(plan.description || '')}</p>
      <ul class="plan-features">${(plan.features || []).map(f => `<li>${escapeHtml(f)}</li>`).join('')}</ul>
      <div class="plan-card-actions"><a href="addPlan.html" class="btn btn-secondary">Edit</a></div>
    `;
    plansList.appendChild(card);
  });
}

function initials(name = '') {
  return name.split(/\s+/).filter(Boolean).slice(0,2).map(x => x[0].toUpperCase()).join('') || 'U';
}
function escapeHtml(value) {
  return String(value ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}
