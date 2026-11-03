(() => {
  const btn = document.getElementById('navToggle');
  const menu = document.getElementById('navMenu');
  if (!btn || !menu) return;
  btn.addEventListener('click', () => {
    const abierto = menu.classList.toggle('abierto');
    btn.setAttribute('aria-expanded', abierto);
  });
})();
