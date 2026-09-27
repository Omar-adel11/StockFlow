export function makeSearchableSelect(select) {
  if (!select || select.dataset.searchableReady === 'true') return;
  select.dataset.searchableReady = 'true';

  const wrapper = document.createElement('div');
  wrapper.className = 'searchable-select';

  const input = document.createElement('input');
  input.type = 'search';
  input.placeholder = select.options[0]?.textContent || 'Search...';
  input.autocomplete = 'off';

  const menu = document.createElement('div');
  menu.className = 'searchable-select-menu';
  menu.setAttribute('role', 'listbox');

  wrapper.append(input, menu);
  select.parentNode.insertBefore(wrapper, select);
  select.classList.add('searchable-select-source');

  const options = () => [...select.options].filter(o => o.value !== '');
  const close = () => menu.classList.remove('open');

  const render = (filter = '') => {
    const normalized = filter.trim().toLowerCase();
    const matches = options().filter(o => !normalized || o.textContent.toLowerCase().includes(normalized));
    menu.innerHTML = '';

    if (!matches.length) {
      const empty = document.createElement('div');
      empty.className = 'searchable-select-empty';
      empty.textContent = 'No matches found';
      menu.appendChild(empty);
      menu.classList.add('open');
      return;
    }

    matches.forEach(option => {
      const item = document.createElement('div');
      item.className = 'searchable-select-option';
      item.textContent = option.textContent;
      item.dataset.value = option.value;
      item.addEventListener('mousedown', e => e.preventDefault());
      item.addEventListener('click', () => {
        select.value = option.value;
        input.value = option.textContent;
        select.dispatchEvent(new Event('change', { bubbles: true }));
        close();
      });
      menu.appendChild(item);
    });
  };

  const sync = () => {
    const option = [...select.options].find(o => o.value === select.value);
    if (!select.value) {
      input.value = '';
      input.placeholder = select.options[0]?.textContent || 'Search or select...';
      return;
    }
    input.value = option?.textContent || '';
    input.placeholder = '';
  };

  input.addEventListener('focus', () => {
    render('');
    menu.classList.add('open');
  });

  input.addEventListener('input', () => {
    render(input.value);
    menu.classList.add('open');
  });

  input.addEventListener('keydown', e => {
    if (e.key === 'Escape') close();
  });

  select.addEventListener('change', sync);
  document.addEventListener('click', e => {
    if (!wrapper.contains(e.target)) close();
  });

  sync();
}

export function refreshSearchableSelect(select) {
  if (!select) return;
  const wrapper = select.previousElementSibling;
  if (wrapper?.classList.contains('searchable-select')) wrapper.remove();
  select.dataset.searchableReady = '';
  select.classList.remove('searchable-select-source');
  makeSearchableSelect(select);
}