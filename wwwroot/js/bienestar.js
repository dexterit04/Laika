(() => {
  const btn = document.getElementById('btnResp');
  const circulo = document.getElementById('circulo');
  const estado = document.getElementById('estadoResp');
  if (!btn) return;
  let activo = false, timer;

  function ciclo() {
    if (!activo) return;
    estado.textContent = 'Inhala por la nariz...';
    circulo.className = 'circulo inhala';
    timer = setTimeout(() => {
      if (!activo) return;
      estado.textContent = 'Suelta el aire despacio...';
      circulo.className = 'circulo exhala';
      timer = setTimeout(ciclo, 6000);
    }, 4000);
  }
  btn.addEventListener('click', () => {
    activo = !activo;
    clearTimeout(timer);
    if (activo) { btn.textContent = 'Detener'; ciclo(); }
    else { btn.textContent = 'Empezar a respirar'; estado.textContent = 'Listo para empezar'; circulo.className = 'circulo'; }
  });
})();
