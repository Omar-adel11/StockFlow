import { fetchPlans, createPlan, updatePlan } from '../services/planService.js';
import { showConfirm, showNotice } from '../utils/ui.js';

const form = document.getElementById('plan-form');
const formStatus = document.getElementById('form-status');
const submitButton = document.getElementById('submit-btn');
const title = document.querySelector('.admin-topbar h1');
const sectionLabel = document.querySelector('.plan-editor-heading .section-label');
const id = new URLSearchParams(location.search).get('id');

function field(name) { return form.elements[name]; }

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
  field('features').value = Array.isArray(plan.features) ? plan.features.join('\n') : '';
  field('isActive').checked = plan.isActive !== false;
  submitButton.textContent = 'Update Plan';
}

form?.addEventListener('submit', async e => {
  e.preventDefault();

  const data = {
    name: field('name').value.trim(),
    price: Number(field('price').value),
    billingCycle: Number(field('billingCycle').value),
    description: field('description').value.trim(),
    isActive: field('isActive').checked,
    features: field('features').value.split('\n').map(x => x.trim()).filter(Boolean)
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