import '../sessions/authGuard.js';
import { showConfirm, showNotice } from '../utils/ui.js';
import { clearSession } from '../sessions/session.js';
import { getMyProfile, updateMyProfile } from '../services/profileService.js';
import { baseUrl } from '../api/apiClient.js';

const form=document.getElementById('profile-form');
const editBtn=document.getElementById('edit-profile-btn');
const cancelBtn=document.getElementById('cancel-profile-btn');
const imageInput=document.getElementById('profile-image-input');
const changeImageBtn=document.getElementById('change-image-btn');
const image=document.getElementById('profile-image');
const initials=document.getElementById('profile-initials');
const summary=document.getElementById('profile-summary');

let currentProfile=null;

function render(p){
  currentProfile=p||currentProfile||{};
  const name=currentProfile.name||'User';
  document.getElementById('profile-name-heading').textContent=name;
  document.getElementById('profile-role-heading').textContent=currentProfile.role||'User';
  document.getElementById('summary-name').textContent=name;
  document.getElementById('summary-email').textContent=currentProfile.email||'';
  document.getElementById('summary-phone').textContent=currentProfile.phoneNumber||'Not provided';
  document.getElementById('summary-role').textContent=currentProfile.role||'User';
  const letters=name.split(/\s+/).filter(Boolean).slice(0,2).map(x=>x[0]).join('').toUpperCase();
  initials.textContent=letters||'U';
  if(currentProfile.imgUrl){
    const raw=String(currentProfile.imgUrl).replace(/^\/+/, '');
    image.src=/^https?:\/\//i.test(String(currentProfile.imgUrl)) ? currentProfile.imgUrl : `${baseUrl}/files/images/${raw}`;
    image.hidden=false; initials.hidden=true;
    image.onerror=()=>{image.hidden=true;initials.hidden=false;};
  }else{image.removeAttribute('src');image.hidden=true;initials.hidden=false;}
}
function edit(){
  const p=currentProfile||{};
  form.elements.name.value=p.name||'';
  form.elements.email.value=p.email||'';
  form.elements.phoneNumber.value=p.phoneNumber||'';
  form.hidden=false;summary.hidden=true;
}
editBtn?.addEventListener('click',edit);
cancelBtn?.addEventListener('click',()=>{form.hidden=true;summary.hidden=false;});
changeImageBtn?.addEventListener('click',()=>imageInput?.click());
imageInput?.addEventListener('change',()=>{const file=imageInput.files?.[0];if(!file)return;image.src=URL.createObjectURL(file);image.hidden=false;initials.hidden=true;});

form?.addEventListener('submit',async e=>{
  e.preventDefault();
  if(!await showConfirm('Save the profile changes?',{confirmText:'Save Changes',danger:false})) return;
  const data=new FormData();
  data.append('Name',form.elements.name.value.trim());
  data.append('Email',form.elements.email.value.trim());
  data.append('PhoneNumber',form.elements.phoneNumber.value.trim());
  if(imageInput?.files?.[0]) data.append('file',imageInput.files[0]);
  try{
    const updated=await updateMyProfile(data);
    currentProfile=updated||{...currentProfile,name:form.elements.name.value.trim(),email:form.elements.email.value.trim(),phoneNumber:form.elements.phoneNumber.value.trim()};
    sessionStorage.setItem('name',currentProfile.name||'');
    sessionStorage.setItem('email',currentProfile.email||'');
    sessionStorage.setItem('imgUrl',currentProfile.imgUrl||'');
    form.hidden=true;summary.hidden=false;render(currentProfile);
    showNotice('Profile updated successfully.');
  }catch(error){showNotice(error.message||'Failed to update profile.','error');}
});

async function init(){
  try{
    currentProfile=await getMyProfile();
    render(currentProfile);
  }catch(error){
    console.error(error);
    render({name:sessionStorage.getItem('name')||'User',email:sessionStorage.getItem('email')||'',role:sessionStorage.getItem('role')||'User'});
    showNotice(error.message||'Failed to load profile.','error');
  }
}
document.getElementById('logout-btn')?.addEventListener('click',()=>{clearSession();location.href='index.html';});
document.getElementById('profile-logout')?.addEventListener('click',()=>{clearSession();location.href='index.html';});
init();
