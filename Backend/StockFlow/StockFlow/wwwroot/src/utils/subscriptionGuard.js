import { getAccessToken } from '../sessions/session.js';
import { getWithAuth, baseUrl } from '../api/apiClient.js';

const PUBLIC_PAGES = new Set(['index.html','login.html','signup.html','forgot-password.html','reset-password.html','otp.html','accept-invite.html','acceptinvite.html','plans.html','payment-success.html','subscription-expired.html','about.html','contact.html']);
function role(){try{const t=getAccessToken();if(!t)return '';const p=JSON.parse(atob(t.split('.')[1].replace(/-/g,'+').replace(/_/g,'/')));return String(p.role||p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']||'').toLowerCase().replace(/\s+/g,'');}catch{return '';}}
function active(s){if(!s)return false;const status=String(s.status??'').toLowerCase();return ['active','trialing','1','5'].includes(status)&&new Date(s.endDateUtc)>new Date();}
async function enforceSubscription(){
 const page=(location.pathname.split('/').pop()||'dashboard.html').toLowerCase();
 if(PUBLIC_PAGES.has(page))return true;
 if(!getAccessToken()){location.href='Login.html';return false;}
 const r=role();
 if(r==='saasadmin')return true;
 try{const s=await getWithAuth(`${baseUrl}/api/Subscriptions/me`);if(active(s))return true;}catch(e){console.warn('Subscription check failed',e);}
 location.href=['businessowner','owner'].includes(r)?'plans.html?required=subscription':'subscription-expired.html';
 return false;
}
enforceSubscription();
export {enforceSubscription,active};