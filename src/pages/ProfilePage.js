import { showConfirm, showNotice } from '../utils/ui.js';
import { clearSession } from '../sessions/session.js';

const form=document.getElementById('profile-form');
const editBtn=document.getElementById('edit-profile-btn');
const cancelBtn=document.getElementById('cancel-profile-btn');
const imageInput=document.getElementById('profile-image-input');
const changeImageBtn=document.getElementById('change-image-btn');
const image=document.getElementById('profile-image');
const initials=document.getElementById('profile-initials');
const summary=document.getElementById('profile-summary');

function values(){return {name:sessionStorage.getItem('name')||'User',email:sessionStorage.getItem('email')||'',role:sessionStorage.getItem('role')||'User',status:sessionStorage.getItem('status')||'Active',imgUrl:sessionStorage.getItem('imgUrl')||''};}
function render(){const v=values();document.getElementById('profile-name-heading').textContent=v.name;document.getElementById('profile-role-heading').textContent=v.role;document.getElementById('summary-name').textContent=v.name;document.getElementById('summary-email').textContent=v.email;document.getElementById('summary-role').textContent=v.role;document.getElementById('summary-status').textContent=v.status;initials.textContent=v.name.split(/\s+/).map(x=>x[0]).slice(0,2).join('').toUpperCase();if(v.imgUrl){image.src=v.imgUrl;image.hidden=false;initials.hidden=true;}else{image.hidden=true;initials.hidden=false;}}
function edit(){const v=values();form.elements.name.value=v.name;form.elements.email.value=v.email;form.elements.role.value=v.role;form.elements.status.value=v.status;form.hidden=false;summary.hidden=true;}
editBtn.addEventListener('click',edit);
cancelBtn.addEventListener('click',()=>{form.hidden=true;summary.hidden=false;});
changeImageBtn.addEventListener('click',()=>imageInput.click());
imageInput.addEventListener('change',()=>{const file=imageInput.files?.[0];if(!file)return;const reader=new FileReader();reader.onload=()=>{sessionStorage.setItem('imgUrl',reader.result);render();showNotice('Profile image updated locally.');};reader.readAsDataURL(file);});
form.addEventListener('submit',async e=>{e.preventDefault();const ok=await showConfirm('Save the profile changes?',{confirmText:'Save Changes',danger:false});if(!ok)return;sessionStorage.setItem('name',form.elements.name.value.trim());sessionStorage.setItem('email',form.elements.email.value.trim());sessionStorage.setItem('role',form.elements.role.value);sessionStorage.setItem('status',form.elements.status.value);form.hidden=true;summary.hidden=false;render();showNotice('Profile updated in this browser session. The backend currently exposes no profile-update endpoint, so no server-side profile mutation was attempted.','warning',5000);});
async function logout(){clearSession();window.location.href='index.html';}
document.getElementById('logout-btn')?.addEventListener('click',logout);document.getElementById('profile-logout')?.addEventListener('click',logout);render();