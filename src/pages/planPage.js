import { validatePlanForm } from '../validation/planValidation.js';
import { fetchPlans, createPlan, updatePlan, deletePlan } from '../services/planService.js';
import { setPlans, getPlanById } from '../state/planState.js';
import { escapeHtml } from '../utils/helpers.js';

const form = document.getElementById('plan-form');
const formTitle = document.getElementById('form-title');
const formStatus = document.getElementById('form-status');
const submitButton = document.getElementById('submit-btn');
const cancelButton = document.getElementById('cancel-btn');

const plansList = document.getElementById('plans-list');
const plansEmpty = document.getElementById('plans-empty');
const plansLoading = document.getElementById('plans-loading');

// null = creating a new plan, otherwise the id of the plan being edited.
// This lives here rather than in state/planState.js because it's
// transient UI state for this one page, not data shared across pages.
let editingId = null;

async function loadPlans() {
    plansLoading.hidden = false;
    plansEmpty.hidden = true;

    try {
        const plans = await fetchPlans();
        setPlans(plans);
        renderPlans(plans);
    } catch (error) {
        console.error(error);
        plansList.innerHTML = '';
        plansEmpty.hidden = false;
        plansEmpty.textContent = "Couldn't load plans. Is the backend running?";
    } finally {
        plansLoading.hidden = true;
    }
}

function renderPlans(plans) {
    plansList.innerHTML = '';

    if (plans.length === 0) {
        plansEmpty.hidden = false;
        plansEmpty.textContent = 'No plans yet. Add your first one above.';
        return;
    }

    plansEmpty.hidden = true;
    plans.forEach(plan => plansList.appendChild(createPlanCard(plan)));
}

function createPlanCard(plan) {
    const card = document.createElement('div');
    card.className = 'card plan-card';

    const statusBadgeClass = plan.isActive ? 'badge-active' : 'badge-inactive';
    const statusText = plan.isActive ? 'Active' : 'Inactive';
    const featuresHtml = plan.features.map(feature => `<li>${escapeHtml(feature)}</li>`).join('');

    card.innerHTML = `
        <div class="plan-card-header">
            <h3>${escapeHtml(plan.name)}</h3>
            <span class="badge ${statusBadgeClass}">${statusText}</span>
        </div>
        <p class="plan-price">$${plan.price.toFixed(2)} / ${plan.billingCycle}</p>
        <p class="plan-description">${escapeHtml(plan.description)}</p>
        <ul class="plan-features">${featuresHtml}</ul>
        <div class="plan-card-actions">
            <button type="button" class="btn btn-secondary btn-edit">Edit</button>
            <button type="button" class="btn btn-danger btn-delete">Delete</button>
        </div>
    `;

    card.querySelector('.btn-edit').addEventListener('click', () => startEdit(plan.id));
    card.querySelector('.btn-delete').addEventListener('click', () => handleDelete(plan.id));

    return card;
}

function startEdit(id) {
    const plan = getPlanById(id);
    if (!plan) return;

    editingId = plan.id;

    form.elements['name'].value = plan.name;
    form.elements['price'].value = plan.price;
    form.elements['billingCycle'].value = plan.billingCycle;
    form.elements['description'].value = plan.description;
    form.elements['features'].value = plan.features.join('\n');
    form.elements['isActive'].checked = plan.isActive;

    formTitle.textContent = 'Edit Plan';
    submitButton.textContent = 'Update Plan';
    cancelButton.hidden = false;

    form.scrollIntoView({ behavior: 'smooth' });
}

function resetForm() {
    form.reset();
    editingId = null;
    formTitle.textContent = 'Add New Plan';
    submitButton.textContent = 'Save Plan';
    cancelButton.hidden = true;
}

cancelButton.addEventListener('click', resetForm);

form.addEventListener('submit', async (event) => {
    event.preventDefault();

    const formData = new FormData(form);
    const errors = validatePlanForm(formData);

    const hasErrors = Object.keys(errors).length > 0;
    if (hasErrors) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    const data = {
        name: formData.get('name').trim(),
        price: parseFloat(formData.get('price')),
        billingCycle: formData.get('billingCycle') === 'Yearly' ? 1 : 0,
        description: formData.get('description').trim(),
        // FormData leaves an unchecked checkbox out entirely rather than
        // sending false for it - so whether the key is present at all
        // is what "checked" means here, not a boolean value inside it.
        isActive: formData.has('isActive'),
        features: formData.get('features')
            .split('\n')
            .map(line => line.trim())
            .filter(line => line.length > 0)
    };

    submitButton.disabled = true;
    formStatus.textContent = 'Saving...';

    try {
        if (editingId) {
            await updatePlan(editingId, data);
            formStatus.textContent = 'Plan updated successfully.';
        } else {
            await createPlan(data);
            formStatus.textContent = 'Plan created successfully.';
        }
        resetForm();
        await loadPlans();
    } catch (error) {
        console.error(error);
        formStatus.textContent = error.message || 'Something went wrong. Please try again.';
    } finally {
        submitButton.disabled = false;
    }
});

async function handleDelete(id) {
    const confirmed = confirm("Delete this plan? This can't be undone.");
    if (!confirmed) return;

    try {
        await deletePlan(id);
        await loadPlans();
    } catch (error) {
        console.error(error);
        alert(`Couldn't delete this plan: ${error.message}`);
    }
}

loadPlans();