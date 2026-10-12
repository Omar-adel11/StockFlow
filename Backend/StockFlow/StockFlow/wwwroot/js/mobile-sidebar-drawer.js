(() => {
  const toggle = document.getElementById("mobile-menu-toggle");
  const sidebar = document.getElementById("app-sidebar");
  const backdrop = document.getElementById("mobile-sidebar-backdrop");
  if (!toggle || !sidebar || !backdrop) return;
  const close = () => {
    document.body.classList.remove("mobile-sidebar-open");
    toggle.setAttribute("aria-expanded", "false");
    toggle.setAttribute("aria-label", "Open navigation menu");
  };
  const open = () => {
    document.body.classList.add("mobile-sidebar-open");
    toggle.setAttribute("aria-expanded", "true");
    toggle.setAttribute("aria-label", "Close navigation menu");
  };
  toggle.addEventListener("click", () => {
    document.body.classList.contains("mobile-sidebar-open") ? close() : open();
  });
  backdrop.addEventListener("click", close);
  sidebar.querySelectorAll("a").forEach(a => a.addEventListener("click", close));
  document.addEventListener("keydown", e => { if (e.key === "Escape") close(); });
  window.addEventListener("resize", () => { if (window.innerWidth > 800) close(); });
})();