import { fetchPlans, subscribeToPlan } from '../services/planService.js';
import { getAccessToken } from '../sessions/session.js';
import { showNotice } from '../utils/ui.js';

const list=document.getElementById('plans-list'), empty=document.getElementById('plans-empty'), loading=document.getElementById('plans-loading');
const esc=v=>String(v??'').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
function cycle(v){return v===1||String(v).toLowerCase()==='yearly'?'Yearly':'Monthly';}
function featureText(f){
  if(typeof f==='string') return f;
  const name=f?.name||f?.featureKey||'Feature';
  const value=f?.value;
  if(!value || String(value).toLowerCase()==='true') return name;
  if(String(value).toLowerCase()==='false') return '';
  return `${name}: ${value}`;
}
async function init(){
  loading.hidden=false; empty.hidden=true;
  try{
    const data=await fetchPlans(); const plans=Array.isArray(data)?data:[];
    list.innerHTML='';
    if(!plans.length){empty.hidden=false;return;}
    plans.filter(p=>p.isActive!==false).forEach(p=>{
      const card=document.createElement('article');card.className='card public-plan-card';
      const features=(p.features||[]).map(featureText).filter(Boolean);
      card.innerHTML='<span class="plan-kicker">StockFlow Plan</span><h2>'+esc(p.name)+'</h2><p class="public-plan-description">'+esc(p.description||'')+'</p><p class="public-plan-price">$'+Number(p.price||0).toFixed(2)+' <span>/ '+cycle(p.billingCycle)+'</span></p><ul class="plan-features">'+features.map(f=>'<li>'+esc(f)+'</li>').join('')+'</ul><button type="button" class="btn btn-primary subscribe-btn">Subscribe</button>';
      card.querySelector('.subscribe-btn').addEventListener('click',()=>subscribe(p));
      list.appendChild(card);
    });
    if(!list.children.length) empty.hidden=false;
  }catch(e){empty.textContent=e.message||'Unable to load plans.';empty.hidden=false;}
  finally{loading.hidden=true;}
}
async function subscribe(plan){
  if(!getAccessToken()){
    sessionStorage.setItem('pendingPlanId', String(plan.id));
    showNotice('Please log in before subscribing to a plan.','error');
    setTimeout(()=>location.href='Login.html',700);
    return;
  }
  try{
    const button=[...document.querySelectorAll('.subscribe-btn')].find(b=>b.closest('.public-plan-card')?.querySelector('h2')?.textContent===plan.name);
    if(button) { button.disabled=true; button.textContent='Redirecting...'; }
    const result=await subscribeToPlan(plan.id);
    if(!result?.paymentUrl) throw new Error('The payment checkout URL was not returned.');
    location.href=result.paymentUrl;
  }catch(error){
    showNotice(error.message||'Unable to start payment.','error');
    const button=[...document.querySelectorAll('.subscribe-btn')].find(b=>b.closest('.public-plan-card')?.querySelector('h2')?.textContent===plan.name);
    if(button) { button.disabled=false; button.textContent='Subscribe'; }
  }
}
init();