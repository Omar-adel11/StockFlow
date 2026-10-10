import '../sessions/authGuard.js';
import { inventoryService } from '../services/InventoryService.js';import { productService } from '../services/ProductService.js';import { warehouseService } from '../services/WarehouseService.js';import { buildAdjustmentPayload, validateStockAdjustment } from '../validation/inventoryValidation.js';import { clearSession } from '../sessions/session.js';
import { initializeAuth } from '../services/authService.js';
const p=document.getElementById('product'),w=document.getElementById('warehouse'),form=document.getElementById('adjust-form'),status=document.getElementById('status');
document.getElementById('logout-btn')?.addEventListener('click',()=>{clearSession();location.href='index.html';});
async function init(){const [ps,ws]=await Promise.all([productService.getAll(),warehouseService.getAll()]);p.innerHTML='<option value="">Select Product</option>'+ps.map(x=>`<option value="${x.id}">${x.name}</option>`).join('');w.innerHTML='<option value="">Select Warehouse</option>'+ws.map(x=>`<option value="${x.id}">${x.name}</option>`).join('');}
document.addEventListener('DOMContentLoaded', async () => {
    // Check/restore authentication session
    const isAuthenticated = await initializeAuth();
    if (!isAuthenticated) return; // Redirects to Login if unauthenticated

    // Run your page initialization
    await init();
});
form.addEventListener('submit',async e=>{e.preventDefault();const data={productId:p.value,warehouseId:w.value,quantityChanged:document.getElementById('quantity').value,reason:document.getElementById('reason').value};const v=validateStockAdjustment(data);if(!v.isValid){status.textContent=v.errors.join(' | ');return;}try{await inventoryService.adjustStock(buildAdjustmentPayload(data));location.href='inventory.html';}catch(err){status.textContent=err.message;}});init().catch(e=>status.textContent=e.message);