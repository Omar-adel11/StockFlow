import '../sessions/authGuard.js';
import { getDashboardSummary } from '../services/dashboardService.js';
import { getAccessToken, clearSession } from '../sessions/session.js';
import { showNotice } from '../utils/ui.js';

const money = value => '$' + Number(value || 0).toLocaleString(undefined,{minimumFractionDigits:2,maximumFractionDigits:2});
const esc = value => String(value ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');

function getRole() {
  const token=getAccessToken();
  if(!token) return '';
  try {
    const p=JSON.parse(atob(token.split('.')[1].replace(/-/g,'+').replace(/_/g,'/')));
    return p.role || p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '';
  } catch { return ''; }
}
function formatDate(value){if(!value)return '';const d=new Date(value);return Number.isNaN(d.getTime())?'':d.toLocaleString([], {month:'short',day:'numeric',hour:'2-digit',minute:'2-digit'});}

function render(data) {
  const name=sessionStorage.getItem('name') || 'there';
  document.getElementById('dashboard-greeting').textContent='Good morning, ' + name + '.';
  document.getElementById('dashboard-greeting-subtitle').textContent='Here is what is happening with your inventory today.';
  document.getElementById('total-products').textContent=Number(data.totalProducts||0).toLocaleString();
  document.getElementById('products-growth').textContent=(data.productsGrowthPercentage>=0?'↑ ':'↓ ') + Math.abs(data.productsGrowthPercentage||0).toFixed(1) + '% vs last month';
  document.getElementById('low-stock-count').textContent=Number(data.lowStockItemsCount||0).toLocaleString();
  document.getElementById('total-orders').textContent=Number(data.totalOrders||0).toLocaleString();
  document.getElementById('orders-growth').textContent=(data.ordersGrowthPercentage>=0?'↑ ':'↓ ') + Math.abs(data.ordersGrowthPercentage||0).toFixed(1) + '% vs last month';
  document.getElementById('total-revenue').textContent=money(data.totalRevenue);
  document.getElementById('revenue-growth').textContent=(data.revenueGrowthPercentage>=0?'↑ ':'↓ ') + Math.abs(data.revenueGrowthPercentage||0).toFixed(1) + '% vs last month';
  document.getElementById('gross-profit').textContent=money(data.grossProfit);

  document.getElementById('sales-bars').innerHTML=(data.monthlySales||[]).map(x =>
    '<div class="sales-bar-wrap"><span>' + money(x.totalSales) + '</span><i style="height:' + Math.max(4,Number(x.heightPercentage||0)) + '%"></i><small>' + esc(x.month) + '</small></div>'
  ).join('');

  document.getElementById('low-stock-list').innerHTML=(data.lowStockAlerts||[]).slice(0,6).map(x =>
    '<div><b>' + esc(x.productName) + '</b><span>' + x.quantityOnHand + ' left</span></div>'
  ).join('') || '<p class="empty-state">No low stock items.</p>';

  document.getElementById('recent-activity-list').innerHTML=(data.recentActivities||[]).slice(0,6).map(x =>
    '<div><b>' + esc(x.title) + '</b><span>' + esc(x.subtitle) + ' · ' + formatDate(x.createdAt) + '</span></div>'
  ).join('') || '<p class="empty-state">No recent activity.</p>';

  document.getElementById('category-list').innerHTML=(data.categoryDistribution||[]).map(x =>
    '<div><b>' + esc(x.categoryName) + '</b><span>' + Number(x.percentage||0).toFixed(1) + '%</span></div>'
  ).join('') || '<p class="empty-state">No category data.</p>';
}
async function init(){
  if(!getAccessToken()){location.href='Login.html';return;}
  if(getRole()==='SaasAdmin'){location.href='adminPanel.html';return;}
  document.getElementById('logout-btn')?.addEventListener('click',()=>{clearSession();location.href='index.html';});
  try { render(await getDashboardSummary()); }
  catch(e){ console.error(e); showNotice(e.message||'Failed to load dashboard.','error'); }
}
document.addEventListener('DOMContentLoaded',init);
