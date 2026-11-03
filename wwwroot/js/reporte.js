(() => {
  const form = document.getElementById('formReporte');
  if (!form) return;
  const err = document.getElementById('errorRep');
  const ok = document.getElementById('exitoRep');

  form.addEventListener('submit', async e => {
    e.preventDefault();
    err.hidden = true;
    try {
      const res = await fetch('/api/reporte', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          tipo: document.getElementById('tipo').value,
          descripcion: document.getElementById('descripcion').value
        })
      });
      const data = await res.json();
      if (!res.ok) { err.textContent = data.error || 'No se pudo enviar.'; err.hidden = false; return; }
      document.getElementById('codigoRep').textContent = data.codigo;
      form.hidden = true;
      ok.hidden = false;
      ok.scrollIntoView({ behavior: 'smooth' });
    } catch {
      err.textContent = 'No se pudo enviar. Intenta de nuevo.';
      err.hidden = false;
    }
  });
})();
