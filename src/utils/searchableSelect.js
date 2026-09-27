export function makeSearchableSelect(select){
 if(!select||select.dataset.searchableReady)return;
 select.dataset.searchableReady='true';
 const wrapper=document.createElement('div'); wrapper.className='searchable-select';
 const input=document.createElement('input'); input.type='search'; input.placeholder=select.options[0]?.textContent||'Search...'; input.setAttribute('autocomplete','off');
 const list=document.createElement('datalist'); list.id=`${select.id}-options`;
 [...select.options].slice(1).forEach(o=>{const opt=document.createElement('option');opt.value=o.textContent;opt.dataset.id=o.value;list.appendChild(opt);});
 input.setAttribute('list',list.id);
 wrapper.append(input,list); select.parentNode.insertBefore(wrapper,select); select.classList.add('searchable-select-source');
 const sync=()=>{const option=[...select.options].find(o=>o.value===select.value);input.value=option?.textContent||'';};
 input.addEventListener('input',()=>{const value=input.value.trim().toLowerCase();const option=[...select.options].slice(1).find(o=>o.textContent.toLowerCase()===value||o.textContent.toLowerCase().includes(value));if(option){select.value=option.value;select.dispatchEvent(new Event('change',{bubbles:true}));}else if(!value){select.value='';select.dispatchEvent(new Event('change',{bubbles:true}));}});
 select.addEventListener('change',sync); sync();
}
export function refreshSearchableSelect(select){if(!select)return;const wrapper=select.previousElementSibling;if(wrapper?.classList.contains('searchable-select'))wrapper.remove();select.dataset.searchableReady='';makeSearchableSelect(select);}