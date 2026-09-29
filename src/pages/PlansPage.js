import { fetchPlans } from '../services/planService.js';
const list=document.getElementById('plans-list'), empty=document.getElementById('plans-empty'), loading=document.getElementById('plans-loading');
const esc=v=>String(v??'').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
function cycle(v){return v===1||String(v).toLowerCase()==='yearly'?'Yearly':'Monthly';}
async function init(){
  loading.hidden=false; empty.hidden=true;
  try{
    const data=await fetchPlans(); const plans=Array.isArray(data)?data:[];
    list.innerHTML='';
    if(!plans.length){empty.hidden=false;return;}
    plans.filter(p=>p.isActive!==false).forEach(p=>{
      const card=document.createElement('article');card.className='card public-plan-card';
      card.innerHTML='<span class="plan-kicker">StockFlow Plan</span><h2>'+esc(p.name)+'</h2><p class="public-plan-description">'+esc(p.description||'')+'</p><p class="public-plan-price">$'+Number(p.price||0).toFixed(2)+' <span>/ '+cycle(p.billingCycle)+'</span></p><ul class="plan-features">'+(p.features||[]).map(f=>'<li>'+esc(f)+'</li>').join('')+'</ul><button type="button" class="btn btn-primary subscribe-btn">Subscribe</button>';
      list.appendChild(card);
    });
    if(!list.children.length) empty.hidden=false;
  }catch(e){empty.textContent=e.message||'Unable to load plans.';empty.hidden=false;}
  finally{loading.hidden=true;}
}
init();
