import { getBusinessOwners, updateBusinessOwnerStatus, assignPlan } from '../services/saasAdminService.js';
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
let selectedOwner=null;

document.addEventListener('DOMContentLoaded', init);

async function init() {
  setupAssignPlanModal();
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
        if (next && !owner.currentPlanId) {
          toggle.checked = false;
          showNotice('Assign a subscription plan before activating this business owner.', 'error');
          return;
        }
        toggle.disabled = true;
        await updateBusinessOwnerStatus(owner.id, next, next ? owner.currentPlanId : null);
        showNotice('Business owner status updated successfully.');
        renderOwners(await getBusinessOwners(ownerSearch?.value || null));
      } catch (error) {
        toggle.checked = !next;
        showNotice(error.message || 'Failed to update status.', 'error');
      } finally {
        toggle.disabled = false;
      }
    });

    row.querySelector('.assign-plan-btn')?.addEventListener('click', () => openAssignPlan(owner));

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
      <div class="plan-card-actions"><a href="addPlan.html?id=${plan.id}" class="btn btn-secondary">Edit</a>
        <button type="button" class="btn btn-danger delete-plan-btn" data-plan-id="${plan.id}">Delete</button></div>
    `;
    card.querySelector(".delete-plan-btn")?.addEventListener("click", async () => {
      const confirmed = await showConfirm("Delete this plan? This cannot be undone.", { confirmText: "Delete Plan" });
      if (!confirmed) return;
      try {
        const { deletePlan } = await import("../services/planService.js");
        await deletePlan(plan.id);
        showNotice("Plan deleted successfully.");
        await loadPlans();
      } catch (error) { showNotice(error.message || "Failed to delete plan.", "error"); }
    });
    plansList.appendChild(card);
  });
}

function initials(name = '') {
  return name.split(/\s+/).filter(Boolean).slice(0,2).map(x => x[0].toUpperCase()).join('') || 'U';
}
function escapeHtml(value) {
  return String(value ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function setupAssignPlanModal(){
  const modal=document.getElementById('assign-plan-modal');
  const form=document.getElementById('assign-plan-form');
  const close=()=>modal?.close();
  modal?.querySelectorAll('.modal-close, .modal-actions button[type="button"]').forEach(b=>b.addEventListener('click',close));
  form?.addEventListener('submit',async e=>{
    e.preventDefault();
    if(!selectedOwner?.businessId) return showNotice('This business owner has no business to assign a plan to.','error');
    const planId=document.getElementById('assign-plan-select').value;
    if(!planId) return showNotice('Select a plan first.','error');
    modal.close();
    const confirmed=await showConfirm('Assign this plan to '+(selectedOwner.fullName||selectedOwner.name)+'?',{confirmText:'Assign Plan',danger:false});
    if(!confirmed) return;
    try{
      const btn=form.querySelector('button[type="submit"]');btn.disabled=true;
      await assignPlan(selectedOwner.businessId,Number(planId));
      modal.close();showNotice('Plan assigned successfully.');
      await loadOwners(ownerSearch?.value||null);
    }catch(error){showNotice(error.message||'Failed to assign plan.','error');}
    finally{form.querySelector('button[type="submit"]').disabled=false;}
  });
}
async function openAssignPlan(owner){
  selectedOwner=owner;
  const modal=document.getElementById('assign-plan-modal');
  const select=document.getElementById('assign-plan-select');
  document.getElementById('assign-plan-owner').textContent='Select a plan for '+(owner.fullName||owner.name||'this business')+'.';
  try{
    const plans=await fetchPlans();
    select.innerHTML='<option value="">Select a plan</option>';
    (Array.isArray(plans)?plans:[]).filter(p=>p.isActive!==false).forEach(p=>{
      const o=document.createElement('option');o.value=p.id;o.textContent=p.name+' — $'+Number(p.price||0).toFixed(2);if(Number(p.id)===Number(owner.currentPlanId))o.selected=true;select.appendChild(o);
    });
    document.getElementById('assign-plan-status').textContent=owner.currentPlanName||'No Plan';
    modal.showModal();
  }catch(error){showNotice(error.message||'Failed to load plans.','error');}
}
