let noticeTimer = null;

function getHost() {
  let host = document.getElementById('page-notice-host');
  if (!host) {
    host = document.createElement('div');
    host.id = 'page-notice-host';
    host.className = 'page-notice-host';
    const main = document.querySelector('.admin-main') || document.body;
    main.prepend(host);
  }
  return host;
}

export function showNotice(message, type = 'success', duration = 3500) {
  const host = getHost();
  host.innerHTML = '';
  const card = document.createElement('div');
  card.className = `page-notice page-notice-${type}`;
  card.setAttribute('role', type === 'error' ? 'alert' : 'status');
  card.innerHTML = `
    <div class="page-notice-content">
      <strong>${type === 'error' ? 'Action failed' : type === 'warning' ? 'Confirmation' : 'Success'}</strong>
      <span></span>
    </div>
    <button type="button" class="page-notice-close" aria-label="Dismiss">&times;</button>
  `;
  card.querySelector('span').textContent = message;
  card.querySelector('.page-notice-close').addEventListener('click', () => {
    host.innerHTML = '';
  });
  host.appendChild(card);

  clearTimeout(noticeTimer);
  if (duration > 0) {
    noticeTimer = setTimeout(() => {
      host.innerHTML = '';
    }, duration);
  }
}

export function showConfirm(message, options = {}) {
  const {
    confirmText = 'Confirm',
    cancelText = 'Cancel',
    danger = true
  } = options;

  return new Promise(resolve => {
    const host = getHost();
    host.innerHTML = '';
    const card = document.createElement('div');
    card.className = 'page-notice page-confirm';
    card.innerHTML = `
      <div class="page-notice-content">
        <strong>Confirm action</strong>
        <span></span>
      </div>
      <div class="page-confirm-actions">
        <button type="button" class="btn btn-secondary page-confirm-cancel">${cancelText}</button>
        <button type="button" class="btn ${danger ? 'btn-danger' : 'btn-primary'} page-confirm-ok">${confirmText}</button>
      </div>
    `;
    card.querySelector('span').textContent = message;

    const finish = value => {
      host.innerHTML = '';
      resolve(value);
    };

    card.querySelector('.page-confirm-cancel').addEventListener('click', () => finish(false));
    card.querySelector('.page-confirm-ok').addEventListener('click', () => finish(true));
    host.appendChild(card);
  });
}
