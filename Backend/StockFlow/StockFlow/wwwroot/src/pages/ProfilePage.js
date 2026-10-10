import '../sessions/authGuard.js';
import { showConfirm, showNotice } from '../utils/ui.js';
import { getAccessToken } from '../sessions/session.js';
import { logout } from '../services/authService.js';
import { getMyProfile, updateMyProfile } from '../services/profileService.js';
import { getWithAuth, postWithAuth, baseUrl } from '../api/apiClient.js';

const form = document.getElementById('profile-form');
const editBtn = document.getElementById('edit-profile-btn');
const cancelBtn = document.getElementById('cancel-profile-btn');
const imageInput = document.getElementById('profile-image-input');
const changeImageBtn = document.getElementById('change-image-btn');
const image = document.getElementById('profile-image');
const initials = document.getElementById('profile-initials');
const summary = document.getElementById('profile-summary');

// Subscription elements
const subSection = document.getElementById('subscription-section');
const subPlanName = document.getElementById('sub-plan-name');
const subBillingCycle = document.getElementById('sub-billing-cycle');
const subStatus = document.getElementById('sub-status');
const subEndDate = document.getElementById('sub-end-date');
const cancelSubBtn = document.getElementById('cancel-sub-btn');

let currentProfile = null;
let currentSubscription = null;

// Subscription status enum map
const SUBSCRIPTION_STATUS_MAP = {
  0: 'Pending',
  1: 'Active',
  2: 'PastDue',
  3: 'Expired',
  4: 'Cancelled',
  5: 'Trialing'
};

function getSubscriptionStatusText(status) {
  if (status === null || status === undefined) return 'Unknown';

  if (Object.prototype.hasOwnProperty.call(SUBSCRIPTION_STATUS_MAP, status)) {
    return SUBSCRIPTION_STATUS_MAP[status];
  }

  return String(status);
}

function getRoleFromToken() {
  try {
    const token = getAccessToken();
    if (!token) return '';
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
    return String(payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '').toLowerCase();
  } catch {
    return '';
  }
}

function render(p) {
  currentProfile = p || currentProfile || {};
  const name = currentProfile.name || 'User';
  document.getElementById('profile-name-heading').textContent = name;
  document.getElementById('profile-role-heading').textContent = currentProfile.role || 'User';
  document.getElementById('summary-name').textContent = name;
  document.getElementById('summary-email').textContent = currentProfile.email || '';
  document.getElementById('summary-phone').textContent = currentProfile.phoneNumber || 'Not provided';
  document.getElementById('summary-role').textContent = currentProfile.role || 'User';

  const letters = name.split(/\s+/).filter(Boolean).slice(0, 2).map(x => x[0]).join('').toUpperCase();
  initials.textContent = letters || 'U';

  if (currentProfile.imgUrl) {
    const raw = String(currentProfile.imgUrl).replace(/^\/+/, '');
    image.src = /^https?:\/\//i.test(String(currentProfile.imgUrl)) ? currentProfile.imgUrl : `${baseUrl}/files/images/${raw}`;
    image.hidden = false;
    initials.hidden = true;
    image.onerror = () => { image.hidden = true; initials.hidden = false; };
  } else {
    image.removeAttribute('src');
    image.hidden = true;
    initials.hidden = false;
  }
}

function renderSubscription(sub) {
  if (!sub || !subSection) return;

  currentSubscription = sub;
  subSection.hidden = false;

  subPlanName.textContent = sub.planName || sub.plan?.name || 'Standard Plan';
  subBillingCycle.textContent = sub.billingCycle || sub.plan?.billingCycle || 'Monthly';

  // Map integer/numeric status code to string representation
  const statusText = getSubscriptionStatusText(sub.status);
  subStatus.textContent = statusText;

  if (sub.endDateUtc || sub.endDate) {
    const dateObj = new Date(sub.endDateUtc || sub.endDate);
    subEndDate.textContent = isNaN(dateObj) ? 'N/A' : dateObj.toLocaleDateString();
  } else {
    subEndDate.textContent = 'N/A';
  }

  // Disable button if subscription is Cancelled (4) or Expired (3)
  const numericStatus = Number(sub.status);
  const isCanceled = numericStatus === 4 || statusText.toLowerCase() === 'cancelled' || statusText.toLowerCase() === 'canceled';

  if (isCanceled) {
    cancelSubBtn.disabled = true;
    cancelSubBtn.textContent = 'Canceled';
  } else {
    cancelSubBtn.disabled = false;
    cancelSubBtn.textContent = 'Cancel Subscription';
  }
}

async function loadSubscriptionInfo() {
  const role = getRoleFromToken();
  if (role === 'saasadmin') return;

  try {
    const sub = await getWithAuth(`${baseUrl}/api/Subscriptions/me`);
    renderSubscription(sub);
  } catch (err) {
    console.warn('Could not retrieve subscription details:', err);
  }
}

async function handleCancelSubscription() {
  const planId = currentSubscription?.planId || currentSubscription?.plan?.id;

  if (!planId) {
    showNotice('Unable to determine current plan ID.', 'error');
    return;
  }

  const confirmed = await showConfirm(
    'Are you sure you want to cancel your subscription? You will lose access to paid features at the end of your billing cycle.',
    { confirmText: 'Yes, Cancel Subscription', danger: true }
  );

  if (!confirmed) return;

  try {
    cancelSubBtn.disabled = true;
    cancelSubBtn.textContent = 'Canceling...';

    // Pass planId as query parameter
    await postWithAuth(`${baseUrl}/api/Subscriptions/cancel?planId=${encodeURIComponent(planId)}`, {});

    showNotice('Subscription canceled successfully.');

    // Refresh subscription details
    await loadSubscriptionInfo();
  } catch (error) {
    cancelSubBtn.disabled = false;
    cancelSubBtn.textContent = 'Cancel Subscription';
    showNotice(error.message || 'Failed to cancel subscription.', 'error');
  }
}

function edit() {
  const p = currentProfile || {};
  form.elements.name.value = p.name || '';
  form.elements.email.value = p.email || '';
  form.elements.phoneNumber.value = p.phoneNumber || '';
  form.hidden = false;
  summary.hidden = true;
}

editBtn?.addEventListener('click', edit);
cancelBtn?.addEventListener('click', () => { form.hidden = true; summary.hidden = false; });
changeImageBtn?.addEventListener('click', () => imageInput?.click());
imageInput?.addEventListener('change', () => {
  const file = imageInput.files?.[0];
  if (!file) return;
  image.src = URL.createObjectURL(file);
  image.hidden = false;
  initials.hidden = true;
});

cancelSubBtn?.addEventListener('click', handleCancelSubscription);

form?.addEventListener('submit', async e => {
  e.preventDefault();
  if (!await showConfirm('Save the profile changes?', { confirmText: 'Save Changes', danger: false })) return;

  const data = new FormData();
  data.append('Name', form.elements.name.value.trim());
  data.append('Email', form.elements.email.value.trim());
  data.append('PhoneNumber', form.elements.phoneNumber.value.trim());
  if (imageInput?.files?.[0]) data.append('file', imageInput.files[0]);

  try {
    const updated = await updateMyProfile(data);
    currentProfile = updated || {
      ...currentProfile,
      name: form.elements.name.value.trim(),
      email: form.elements.email.value.trim(),
      phoneNumber: form.elements.phoneNumber.value.trim()
    };
    sessionStorage.setItem('name', currentProfile.name || '');
    sessionStorage.setItem('email', currentProfile.email || '');
    sessionStorage.setItem('imgUrl', currentProfile.imgUrl || '');

    form.hidden = true;
    summary.hidden = false;
    render(currentProfile);
    showNotice('Profile updated successfully.');
  } catch (error) {
    showNotice(error.message || 'Failed to update profile.', 'error');
  }
});

async function init() {
  try {
    currentProfile = await getMyProfile();
    render(currentProfile);
  } catch (error) {
    console.error(error);
    render({
      name: sessionStorage.getItem('name') || 'User',
      email: sessionStorage.getItem('email') || '',
      role: sessionStorage.getItem('role') || 'User'
    });
    showNotice(error.message || 'Failed to load profile.', 'error');
  }

  await loadSubscriptionInfo();
}

document.getElementById('logout-btn')?.addEventListener('click', () => { void logout(); });
document.getElementById('profile-logout')?.addEventListener('click', () => { void logout(); });

init();