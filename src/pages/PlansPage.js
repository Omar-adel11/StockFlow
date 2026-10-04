import { fetchPlans, subscribeToPlan, startFreeTrial } from '../services/planService.js';
import { getAccessToken } from '../sessions/session.js';
import { showNotice } from '../utils/ui.js';

const list=document.getElementById('plans-list'), empty=document.getElementById('plans-empty'), loading=document.getElementById('plans-loading');
const esc=v=>String(v??'').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
function cycle(v){return v===1||String(v).toLowerCase()==='yearly'?'Yearly':'Monthly';}
function featureText(f){if(typeof f==='string')return f;const name=f?.name||f?.featureKey||'Feature';const value=f?.value;if(!value||String(value).toLowerCase()==='true')return name;if(String(value).toLowerCase()==='false')return '';return `${name}: ${value}`;}
function getRole(){try{const t=getAccessToken();if(!t)return '';const p=JSON.parse(atob(t.split('.')[1].replace(/-/g,'+').replace(/_/g,'/')));return String(p.role||p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']||'').toLowerCase().replace(/\s+/g,'');}catch{return '';}}
function isOwner(){return ['businessowner','owner'].includes(getRole());}
async function init(){
 loading.hidden=false;empty.hidden=true;
 try{
  const data=await fetchPlans();list.innerHTML='';
  const plans=(Array.isArray(data)?data:[]).filter(p=>p.isActive!==false);
  if(!plans.length){empty.hidden=false;return;}
  plans.forEach(p=>{
   const trial=p.isFreeTrial===true;
   const features=(p.features||[]).filter(f=>f?.isActive!==false).map(featureText).filter(Boolean);
   const card=document.createElement('article');card.className='card public-plan-card';
   card.innerHTML='<span class="plan-kicker">StockFlow Plan</span><h2>'+esc(p.name)+'</h2><p class="public-plan-description">'+esc(p.description||'')+'</p><p class="public-plan-price">$'+Number(p.price||0).toFixed(2)+' <span>/ '+cycle(p.billingCycle)+'</span></p>'+(trial?'<p class="trial-note">14-day free trial</p>':'')+'<ul class="plan-features">'+features.map(f=>'<li>'+esc(f)+'</li>').join('')+'</ul><button type="button" class="btn btn-primary subscribe-btn">'+(trial?'Start Free Trial':'Subscribe')+'</button>';
   const button=card.querySelector('button');
   if(getAccessToken()&&!isOwner()){button.disabled=true;button.textContent='Business owner only';}
   button.addEventListener('click',()=>startSubscription(p,button));
   list.appendChild(card);
  });
  if(!list.children.length)empty.hidden=false;
 }catch(e){empty.textContent=e.message||'Unable to load plans.';empty.hidden=false;}
 finally{loading.hidden=true;}
}
async function startSubscription(plan,button){
 if(!getAccessToken()){sessionStorage.setItem('pendingPlanId',String(plan.id));location.href='Login.html';return;}
 if(!isOwner()){showNotice('Only the Business Owner can start a trial or pay for a subscription.','error');return;}
 button.disabled=true;button.textContent='Please wait...';
 try{
  if(plan.isFreeTrial===true){
   await startFreeTrial(plan.id);
   location.href='payment-success.html?trial=started';
   return;
  }
  const result=await subscribeToPlan(plan.id,'Paymob',false);
  if(!result?.paymentUrl)throw new Error('The payment checkout URL was not returned.');
  location.href=result.paymentUrl;
 }catch(error){showNotice(error.message||'Unable to start subscription.','error');button.disabled=false;button.textContent=plan.isFreeTrial?'Start Free Trial':'Subscribe';}
}
init();