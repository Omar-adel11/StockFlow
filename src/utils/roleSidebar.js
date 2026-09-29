import { getAccessToken, clearSession } from '../sessions/session.js';

function getRole() {
  const token = getAccessToken();
  if (!token) return String(sessionStorage.getItem('role') || '').toLowerCase();
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
    return String(
      payload.role ||
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      sessionStorage.getItem('role') ||
      ''
    ).toLowerCase();
  } catch {
    return String(sessionStorage.getItem('role') || '').toLowerCase();
  }
}

function currentPage() {
  return location.pathname.split('/').pop().toLowerCase() || 'dashboard.html';
}

function renderSaasSidebar() {
  if (!['saasadmin', 'saas admin'].includes(getRole())) return;

  const sidebar = document.querySelector('.admin-sidebar');
  if (!sidebar) return;

  const page = currentPage();
  sidebar.innerHTML = `
    <div class="sidebar-logo">
      <img src="images/logo.jpg" alt="StockFlow">
      StockFlow
    </div>
    <ul class="sidebar-nav">
      <li><a href="adminPanel.html#overview" class="${page === 'adminpanel.html' ? 'active' : ''}"><span class="nav-icon">▦</span> Overview</a></li>
      <li><a href="adminPanel.html#business-owners"><span class="nav-icon">♙</span> Business Owners</a></li>
      <li><a href="adminPanel.html#plans"><span class="nav-icon">◆</span> Plans</a></li>
    </ul>
    <div class="sidebar-footer">
      <a href="profile.html" class="${page === 'profile.html' ? 'active' : ''}">Profile</a>
      <button type="button" id="logout-btn">Log Out</button>
    </div>
  `;

  document.getElementById('logout-btn')?.addEventListener('click', () => {
    clearSession();
    location.href = 'index.html';
  });
}

document.addEventListener('DOMContentLoaded', renderSaasSidebar);
