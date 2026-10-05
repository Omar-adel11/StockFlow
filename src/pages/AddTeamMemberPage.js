import '../sessions/authGuard.js';
import { teamService } from '../services/teamService.js';
import { showNotice } from '../utils/ui.js';
import { clearSession } from '../sessions/session.js';

const form = document.getElementById('invite-form');
const status = document.getElementById('status');

document.getElementById('logout-btn')?.addEventListener('click', () => {
  clearSession();
  location.href = 'index.html';
});

form?.addEventListener('submit', async (event) => {
  event.preventDefault();
  status.textContent = '';

  const email = document.getElementById('email').value.trim();
  const role = document.getElementById('role').value;
  const submit = form.querySelector('button[type="submit"]');

  try {
    submit.disabled = true;
    const response = await teamService.invite({ email, role });

    showNotice(response?.message || 'Invitation sent successfully.');
    status.textContent = 'Invitation sent successfully. Redirecting...';
    status.className = 'form-status success';

    setTimeout(() => {
      window.location.href = 'team.html';
    }, 1200);
  } catch (error) {
    status.textContent = error.message || 'Failed to send invitation.';
    status.className = 'form-status error';
  } finally {
    submit.disabled = false;
  }
});