import { getAccessToken, clearSession } from '../sessions/session.js';
import { getWithAuth } from '../api/apiClient.js';

const PUBLIC_PAGES = new Set([
  'index.html', 'login.html', 'signup.html', 'forgot-password.html',
  'reset-password.html', 'otp.html', 'accept-invite.html', 'acceptinvite.html',
  'plans.html', 'payment-success.html', 'about.html', 'contact.html'
]);

function getRole() {
  const token = getAccessToken();
  try {
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      return String(
        payload.role ||
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        ''
      ).toLowerCase().replace(/\s+/g, '');
    }
  } catch {}
  return '';
}

function currentPage() {
  return (location.pathname.split('/').pop() || 'dashboard.html').toLowerCase();
}

function isActiveSubscription(subscription) {
  if (!subscription) return false;

  const status = String(subscription.status ?? '').toLowerCase();
  const activeStatus = status === 'active' || status === '1';
  const endDate = new Date(subscription.endDateUtc);

  return activeStatus && !Number.isNaN(endDate.getTime()) && endDate > new Date();
}

async function enforceSubscription() {
  const page = currentPage();
  if (PUBLIC_PAGES.has(page)) return true;

  const token = getAccessToken();
  if (!token) {
    location.href = 'Login.html';
    return false;
  }

  // SaaS Admin is the platform administrator and does not need a tenant subscription.
  if (getRole() === 'saasadmin') return true;

  // Do not loop if already on pricing/payment pages.
  if (page === 'plans.html' || page === 'payment-success.html') return true;

  try {
    const subscription = await getWithAuth('/api/Subscriptions/me');
    if (isActiveSubscription(subscription)) return true;
  } catch (error) {
    // No subscription / expired subscription is handled by redirecting to pricing.
    console.warn('Subscription check failed:', error);
  }

  location.href = 'plans.html?required=subscription';
  return false;
}

enforceSubscription();

export { enforceSubscription, isActiveSubscription };
