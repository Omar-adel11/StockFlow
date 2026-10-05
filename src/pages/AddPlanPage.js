import '../sessions/authGuard.js';
import { fetchPlans, createPlan, updatePlan } from '../services/planService.js';
import { showConfirm, showNotice } from '../utils/ui.js';

const form = document.getElementById('plan-form');
const formStatus = document.getElementById('form-status');
const submitButton = document.getElementById('submit-btn');
const title = document.querySelector('.admin-topbar h1');
const sectionLabel = document.querySelector('.plan-editor-heading .section-label');
const id = new URLSearchParams(location.search).get('id');

function field(name) { return form.elements[name]; }

function buildFeatures() {
  const features = [];
  const warehouseMode = document.getElementById('max-warehouses-mode').value;
  const warehouseValue = warehouseMode === 'unlimited' ? 'Unlimited' : document.getElementById('max-warehouses-value').value.trim();
  if (warehouseValue) features.push({ featureKey: 'MAX_WAREHOUSES', value: warehouseValue, name: 'Maximum Warehouses', isActive: true });

  const usersMode = document.getElementById('max-users-mode').value;
  const usersValue = usersMode === 'unlimited' ? 'Unlimited' : document.getElementById('max-users-value').value.trim();
  if (usersValue) features.push({ featureKey: 'MAX_USERS', value: usersValue, name: 'Maximum Users', isActive: true });

  return features;
}

function loadFeatures(features = []) {
  const map = new Map((features || []).map(f => [String(f.featureKey || '').toUpperCase(), f]));
  for (const [key, modeId, valueId] of [
    ['MAX_WAREHOUSES', 'max-warehouses-mode', 'max-warehouses-value'],
    ['MAX_USERS', 'max-users-mode', 'max-users-value']
  ]) {
    const feature = map.get(key);
    const mode = document.getElementById(modeId);
    const input = document.getElementById(valueId);
    if (!feature) continue;
    if (String(feature.value).toLowerCase() === 'unlimited') {
      mode.value = 'unlimited';
      input.disabled = true;
      input.value = '';
    } else {
      mode.value = 'limit';
      input.disabled = false;
      input.value = feature.value || '';
    }
  }
}

function wireFeatureModes() {
  [['max-warehouses-mode','max-warehouses-value'],['max-users-mode','max-users-value']].forEach(([modeId,inputId])=>{
    const mode=document.getElementById(modeId), input=document.getElementById(inputId);
    mode.addEventListener('change',()=>{ input.disabled=mode.value==='unlimited'; if(input.disabled) input.value=''; });
  });
}

async function init() {
  if (!id) return;
  const plans = await fetchPlans();
  const plan = (plans || []).find(p => Number(p.id) === Number(id));
  if (!plan) {
    formStatus.textContent = 'Plan not found.';
    return;
  }

  title.textContent = 'Edit Plan';
  sectionLabel.textContent = 'Subscription';
  document.querySelector('.plan-editor-heading h2').textContent = 'Edit plan details';
  field('name').value = plan.name || '';
  field('price').value = plan.price ?? '';
  const cycle = plan.billingCycle;
  field('billingCycle').value =
    cycle === 1 || String(cycle).toLowerCase() === 'yearly' ? '1' : '0';
  field('description').value = plan.description || '';
  loadFeatures(plan.features);
  field('isActive').checked = plan.isActive !== false;
  field('isFreeTrial').checked = plan.isFreeTrial === true;
  submitButton.textContent = 'Update Plan';
}

wireFeatureModes();

form?.addEventListener('submit', async e => {
  e.preventDefault();

  const data = {
    name: field('name').value.trim(),
    price: Number(field('price').value),
    billingCycle: field('billingCycle').value === '1' ? 1 : 0,
    description: field('description').value.trim(),
    isActive: field('isActive').checked,
    isFreeTrial: field('isFreeTrial').checked,
    features: buildFeatures()
  };

  if (!data.name || Number.isNaN(data.price)) {
    formStatus.textContent = 'Name and price are required.';
    return;
  }

  try {
    if (id) {
      const confirmed = await showConfirm('Update this plan?', { confirmText: 'Update Plan', danger: false });
      if (!confirmed) return;
      await updatePlan(id, data);
      showNotice('Plan updated successfully.');
    } else {
      await createPlan(data);
      showNotice('Plan created successfully.');
    }
    setTimeout(() => location.href = 'adminPanel.html#plans', 700);
  } catch (error) {
    formStatus.textContent = error.message || 'Failed to save plan.';
  }
});

init().catch(error => { formStatus.textContent = error.message || 'Failed to load plan.'; });