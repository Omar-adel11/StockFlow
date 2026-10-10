let noticeTimer = null;
let noticeHost;

function getNoticeHost() {
  let host = document.getElementById('page-notice-host');
  if (!host) {
    host = document.createElement('div');
    host.id = 'page-notice-host';
    host.className = 'page-notice-host';
    document.body.appendChild(host);
  }
  return host;
}

export function showNotice(message, type = 'success', duration = 3500) {
  const host = getNoticeHost();
  host.innerHTML = '';
  const card = document.createElement('div');
  card.className = `page-notice page-notice-${type}`;
  card.setAttribute('role', type === 'error' ? 'alert' : 'status');
  card.innerHTML = `
    <div class="page-notice-content">
      <strong>${type === 'error' ? 'Action failed' : type === 'warning' ? 'Notice' : 'Success'}</strong>
      <span></span>
    </div>
    <button type="button" class="page-notice-close" aria-label="Dismiss">&times;</button>
  `;
  card.querySelector('span').textContent = message;
  card.querySelector('.page-notice-close').addEventListener('click', () => host.replaceChildren());
  host.appendChild(card);
  clearTimeout(noticeTimer);
  if (duration > 0) noticeTimer = setTimeout(() => host.replaceChildren(), duration);
}

export function showConfirm(message, options = {}) {
  const { confirmText = 'Confirm', cancelText = 'Cancel', danger = true, title = 'Confirm action' } = options;
  return new Promise(resolve => {
    const host = getNoticeHost();
    host.replaceChildren();
    const backdrop = document.createElement('div');
    backdrop.className = 'confirmation-overlay';
    const card = document.createElement('div');
    card.className = 'confirmation-card';
    card.setAttribute('role', 'dialog');
    card.setAttribute('aria-modal', 'true');
    card.innerHTML = `
      <div class="confirmation-content">
        <span class="confirmation-icon">!</span>
        <div><h3></h3><p></p></div>
      </div>
      <div class="confirmation-actions">
        <button type="button" class="btn btn-secondary confirmation-cancel"></button>
        <button type="button" class="btn ${danger ? 'btn-danger' : 'btn-primary'} confirmation-ok"></button>
      </div>`;
    card.querySelector('h3').textContent = title;
    card.querySelector('p').textContent = message;
    card.querySelector('.confirmation-cancel').textContent = cancelText;
    card.querySelector('.confirmation-ok').textContent = confirmText;
    backdrop.appendChild(card);
    const finish = value => { host.replaceChildren(); resolve(value); };
    card.querySelector('.confirmation-cancel').addEventListener('click', () => finish(false));
    card.querySelector('.confirmation-ok').addEventListener('click', () => finish(true));
    backdrop.addEventListener('click', e => { if (e.target === backdrop) finish(false); });
    host.appendChild(backdrop);
    card.querySelector('.confirmation-cancel').focus();
  });
}
