import { getAccessToken } from '../sessions/session.js';
import { logout } from '../services/authService.js';

function getRole() {
  const token = getAccessToken();
  try {
    if (token) {
      const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const payload = JSON.parse(atob(base64));
      return String(
        payload.role ||
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        ''
      ).toLowerCase().replace(/\\s+/g, '');
    }
  } catch {}
  return String(sessionStorage.getItem('role') || '').toLowerCase().replace(/\\s+/g, '');
}

function currentPage() {
  return location.pathname.split('/').pop().toLowerCase() || 'dashboard.html';
}

function renderSidebar() {
  const sidebar = document.querySelector('.admin-sidebar');
  if (!sidebar) return;

  const role = getRole();
  const isSaasAdmin = role === 'saasadmin';
  const page = currentPage();

  if (isSaasAdmin) {
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
    `;
  } else {
    sidebar.innerHTML = `
      <div class="sidebar-logo">
        <img src="images/logo.jpg" alt="StockFlow">
        StockFlow
      </div>
      <ul class="sidebar-nav">
        <li><a href="dashboard.html" class="${page === 'dashboard.html' ? 'active' : ''}"><span class="nav-icon">▦</span> Dashboard</a></li>
        <li><a href="categories.html" class="${page === 'categories.html' ? 'active' : ''}"><span class="nav-icon">📁</span> Categories</a></li>
        <li><a href="products.html" class="${page === 'products.html' ? 'active' : ''}"><span class="nav-icon">📦</span> Products</a></li>
        <li><a href="inventory.html" class="${page === 'inventory.html' ? 'active' : ''}"><span class="nav-icon">🏷️</span> Inventory</a></li>
        <li><a href="warehouses.html" class="${page === 'warehouses.html' ? 'active' : ''}"><span class="nav-icon">🏢</span> Warehouses</a></li>
        <li><a href="suppliers.html" class="${page === 'suppliers.html' ? 'active' : ''}"><span class="nav-icon">🤝</span> Suppliers</a></li>
        <li><a href="customers.html" class="${page === 'customers.html' ? 'active' : ''}"><span class="nav-icon">👥</span> Customers</a></li>
        <li><a href="purchase-orders.html" class="${page === 'purchase-orders.html' ? 'active' : ''}"><span class="nav-icon">📋</span> Purchase Orders</a></li>
        <li><a href="sales-orders.html" class="${page === 'sales-orders.html' ? 'active' : ''}"><span class="nav-icon">🛒</span> Sales Orders</a></li>
        <li><a href="team.html" class="${page === 'team.html' ? 'active' : ''}"><span class="nav-icon">👤</span> Team Members</a></li>
      </ul>
    `;
  }

  const footer = document.createElement('div');
  footer.className = 'sidebar-footer';
  footer.innerHTML = `
    <a href="profile.html" class="${page === 'profile.html' ? 'active' : ''}">Profile</a>
    <button type="button" id="logout-btn">Log Out</button>
  `;
  sidebar.appendChild(footer);

}

// Capture logout clicks before any page-level handlers can clear the session.
document.addEventListener('click', event => {
  const button = event.target.closest('#logout-btn, #profile-logout');
  if (!button || button.dataset.logoutPending === 'true') return;

  event.preventDefault();
  event.stopImmediatePropagation();
  button.dataset.logoutPending = 'true';
  void logout().finally(() => { button.dataset.logoutPending = 'false'; });
}, true);

document.addEventListener('DOMContentLoaded', renderSidebar);
