import { baseUrl } from '../api/apiClient.js';

const token = new URLSearchParams(location.search).get('token');
const details = document.getElementById('invite-details');
const form = document.getElementById('accept-invite-form');
const status = document.getElementById('invite-status');

function readError(data, fallback) {
  if (data?.errors && typeof data.errors === 'object') {
    return Object.entries(data.errors)
      .map(([field, messages]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`)
      .join(' | ');
  }
  return data?.message || data?.detail || data?.title || data?.errorMessage || fallback;
}

async function init() {
  if (!token) {
    details.textContent = 'Invitation token is missing.';
    form.hidden = true;
    return;
  }

  const response = await fetch(`${baseUrl}/api/Team/invite/${encodeURIComponent(token)}`);
  const data = await response.json().catch(() => null);
  if (!response.ok || !data?.isValid) {
    details.textContent = readError(data, 'This invitation is invalid or expired.');
    form.hidden = true;
    return;
  }

  details.textContent = `You were invited to ${data.businessName} as ${data.role}. Account email: ${data.email}.`;
  form.hidden = false;
}

form?.addEventListener('submit', async event => {
  event.preventDefault();
  status.textContent = '';

  const name = document.getElementById('invite-name').value.trim();
  const phone = document.getElementById('invite-phone').value.trim();
  const password = document.getElementById('invite-password').value;
  if (!name || !phone || password.length < 6) {
    status.textContent = 'Enter your name, phone number, and a password of at least 6 characters.';
    return;
  }

  const submit = form.querySelector('button[type="submit"]');
  submit.disabled = true;
  submit.textContent = 'Accepting invitation…';

  const formData = new FormData();
  formData.append('Token', token);
  formData.append('Name', name);
  formData.append('PhoneNumber', phone);
  formData.append('Password', password);
  const file = document.getElementById('invite-file').files[0];
  if (file) formData.append('File', file);

  try {
    const response = await fetch(`${baseUrl}/api/Team/accept-invite`, {
      method: 'POST',
      body: formData
    });
    const data = await response.json().catch(() => null);
    if (!response.ok) throw new Error(readError(data, `Invitation acceptance failed (HTTP ${response.status}).`));

    status.textContent = 'Invitation accepted. You can now log in.';
    form.reset();
    form.hidden = true;
  } catch (error) {
    status.textContent = error.message || 'Failed to accept invitation.';
  } finally {
    submit.disabled = false;
    submit.textContent = 'Accept Invitation';
  }
});

init().catch(error => {
  details.textContent = error.message || 'Unable to validate this invitation.';
});
