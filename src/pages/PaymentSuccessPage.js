import { getMySubscription } from '../services/planService.js';

const details=document.getElementById('subscription-details');
const params=new URLSearchParams(location.search);
const paymentSuccess=params.get('success');
const paymentFailed=params.get('success')==='false';

function render(message){details.innerHTML=message;}

async function load(){
  if(paymentFailed){
    document.querySelector('h1').textContent='Payment Failed';
    document.querySelector('.payment-icon').textContent='!';
    render('<p>Your payment was not completed. You can return to Pricing and try again.</p>');
    return;
  }

  // Paymob's redirect is for user experience; the backend webhook is the source of truth.
  // Poll briefly because the webhook may finish just after the browser redirect.
  for(let attempt=0;attempt<6;attempt++){
    try{
      const sub=await getMySubscription();
      if(sub){
        const end=new Date(sub.endDateUtc);
        const status=String(sub.status||'');
        if(status.toLowerCase()==='active' && end>new Date()){
          render(`<p><strong>Plan:</strong> ${escapeHtml(sub.planName)}</p><p><strong>Status:</strong> <span class="status">Active</span></p><p><strong>Valid until:</strong> ${escapeHtml(end.toLocaleDateString())}</p>`);
          return;
        }
      }
    }catch{}
    await new Promise(resolve=>setTimeout(resolve,1500));
  }

  render('<p>Your payment was submitted, but the subscription is still being confirmed. Please check your dashboard shortly.</p>');
}
function escapeHtml(v){return String(v??'').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;')}
load();
