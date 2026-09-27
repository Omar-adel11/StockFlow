import { teamService } from '../services/teamService.js';
import { showConfirm, showNotice } from '../utils/ui.js';
import * as authService from '../services/authService.js';
import { getAccessToken } from '../sessions/session.js';

const membersList = document.getElementById('team-members-list');
const membersEmpty = document.getElementById('team-empty');
const membersLoading = document.getElementById('team-loading');
const invitesList = document.getElementById('pending-invites-list');
const invitesEmpty = document.getElementById('invites-empty');
const invitesLoading = document.getElementById('invites-loading');
const searchInput = document.getElementById('team-search');
const inviteForm = document.getElementById('invite-form');
const inviteModal = document.getElementById('invite-modal');
const roleModal = document.getElementById('role-modal');
let searchTimer;
let editingMemberId = null;

document.addEventListener('DOMContentLoaded', init);

function currentRole() { try { const token=getAccessToken(); if(!token) return ''; const p=JSON.parse(atob(token.split('.')[1].replace(/-/g,'+').replace(/_/g,'/'))); return p.role || p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || ''; } catch { return ''; } }

async function init() {
  document.getElementById('logout-btn')?.addEventListener('click', () => authService.logout());
  document.querySelectorAll('[data-open-invite]').forEach(btn => btn.addEventListener('click', () => inviteModal?.showModal()));
  document.querySelectorAll('.modal-close, .modal .btn-secondary').forEach(btn => btn.addEventListener('click', () => btn.closest('dialog')?.close()));
  searchInput?.addEventListener('input', () => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => loadMembers(searchInput.value), 300);
  });
  inviteForm?.addEventListener('submit', handleInvite);
  document.getElementById('role-form')?.addEventListener('submit', handleRole);
  await Promise.all([loadMembers(), loadInvites()]);
}

async function loadMembers(search = null) {
  membersLoading.hidden = false;
  membersEmpty.hidden = true;
  try {
    const members = await teamService.getMembers(search);
    renderMembers(Array.isArray(members) ? members : []);
  } catch (error) {
    membersList.innerHTML = '';
    membersEmpty.textContent = error.message || 'Failed to load team members.';
    membersEmpty.hidden = false;
  } finally {
    membersLoading.hidden = true;
  }
}

function renderMembers(members) {
  membersList.innerHTML = '';
  if (!members.length) {
    membersEmpty.hidden = false;
    return;
  }
  membersEmpty.hidden = true;
  let managers = 0, staff = 0;
  members.forEach(member => {
    if (member.role === 'Manager') managers++; else if (member.role === 'Staff') staff++;
    const row = document.createElement('tr');
    row.innerHTML = `
      <td><div class="person-cell"><span class="avatar">${initials(member.name)}</span><div><strong>${escapeHtml(member.name)}</strong><span>Member #${member.id}</span></div></div></td>
      <td>${escapeHtml(member.email)}</td>
      <td><span class="role-badge role-${String(member.role || '').toLowerCase()}">${escapeHtml(member.role)}</span></td>
      <td class="table-actions"><button type="button" class="btn btn-secondary btn-small change-role-btn">Change Role</button><button type="button" class="btn btn-danger btn-small remove-member-btn">Remove</button></td>
    `;
    row.querySelector('.change-role-btn').addEventListener('click', () => {
      editingMemberId = member.id;
      document.getElementById('role-member-name').textContent = `Update ${member.name}'s role.`;
      document.getElementById('member-role').value = member.role;
      roleModal?.showModal();
    });
    row.querySelector('.remove-member-btn').addEventListener('click', async () => {
      const confirmed = await showConfirm(`Remove ${member.name} from the team?`, { confirmText: 'Remove Member' });
      if (!confirmed) return;
      try {
        await teamService.removeMember(member.id);
        showNotice('Team member removed successfully.');
        await loadMembers(searchInput?.value || null);
      } catch (error) {
        showNotice(error.message || 'Failed to remove team member.', 'error');
      }
    });
    membersList.appendChild(row);
  });
  document.getElementById('team-member-count').textContent = members.length;
  document.getElementById('manager-count').textContent = managers;
  document.getElementById('staff-count').textContent = staff;
}

async function loadInvites() {
  invitesLoading.hidden = false;
  invitesEmpty.hidden = true;
  try {
    const invites = await teamService.getInvites();
    invitesList.innerHTML = '';
    if (!invites?.length) {
      invitesEmpty.hidden = false;
      return;
    }
    invites.forEach(invite => {
      const row = document.createElement('tr');
      row.innerHTML = `<td>${escapeHtml(invite.email)}</td><td><span class="role-badge role-${String(invite.role || 'Staff').toLowerCase()}">${escapeHtml(invite.role || 'Staff')}</span></td><td>${formatDate(invite.createdAt)}</td><td>${formatDate(invite.expiresAt)}</td><td class="table-actions"><button type="button" class="btn btn-danger btn-small revoke-invite-btn">Revoke</button></td>`;
      row.querySelector('.revoke-invite-btn').addEventListener('click', async () => {
        const confirmed = await showConfirm(`Revoke the invitation for ${invite.email}?`, { confirmText: 'Revoke Invite' });
        if (!confirmed) return;
        try {
          await teamService.cancelInvite(invite.id);
          showNotice('Invitation revoked successfully.');
          await loadInvites();
        } catch (error) {
          showNotice(error.message || 'Failed to revoke invitation.', 'error');
        }
      });
      invitesList.appendChild(row);
    });
    document.getElementById('pending-invite-count').textContent = invites.length;
  } catch (error) {
    invitesList.innerHTML = '';
    invitesEmpty.textContent = error.message || 'Failed to load invitations from the server.';
    invitesEmpty.hidden = false;
  } finally {
    invitesLoading.hidden = true;
  }
}

async function handleInvite(event) {
  event.preventDefault();
  const email = document.getElementById('invite-email').value.trim();
  const role = document.getElementById('invite-role').value;
  if (!email) return;
  try {
    await teamService.invite({ email, role });
    inviteForm.reset();
    inviteModal?.close();
    showNotice('Invitation sent successfully.');
    await loadInvites();
  } catch (error) {
    showNotice(error.message || 'Failed to send invitation.', 'error');
  }
}

async function handleRole(event) {
  event.preventDefault();
  if (!editingMemberId) return;
  const role = document.getElementById('member-role').value;
  try {
    await teamService.updateRole(editingMemberId, role);
    roleModal?.close();
    showNotice('Team member role updated successfully.');
    await loadMembers(searchInput?.value || null);
  } catch (error) {
    showNotice(error.message || 'Failed to update team member role.', 'error');
  }
}

function formatDate(value) {
  if (!value) return '—';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '—' : date.toLocaleDateString();
}
function initials(name = '') {
  return name.split(/\s+/).filter(Boolean).slice(0,2).map(x => x[0].toUpperCase()).join('') || 'U';
}
function escapeHtml(value) {
  return String(value ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}
