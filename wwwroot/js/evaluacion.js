(() => {
  const form = document.getElementById('formEval');
  const out = document.getElementById('resultado');
  if (!form) return;

  form.addEventListener('submit', async e => {
    e.preventDefault();
    const respuestas = [];
    for (let i = 0; i < 8; i++) {
      respuestas.push(Number(form.querySelector(`input[name="p${i}"]:checked`).value));
    }
    out.hidden = false;
    out.className = 'card resultado';
    out.textContent = 'Calculando...';
    try {
      const res = await fetch('/api/evaluacion', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ respuestas })
      });
      if (!res.ok) throw new Error();
      const d = await res.json();
      out.className = 'card resultado ' + d.nivelRiesgo.toLowerCase();
      out.replaceChildren();
      const h = document.createElement('h2');
      h.textContent = 'Nivel de riesgo: ' + d.nivelRiesgo;
      const p = document.createElement('p');
      p.style.whiteSpace = 'pre-wrap';
      p.textContent = d.recomendacion;
      out.append(h, p);
      if (d.alertaSeguridad) {
        const a = document.createElement('p');
        a.innerHTML = '<strong>Respondiste que has pensado en hacerte daño. Importa mucho cómo te sientes.</strong> Habla hoy con un adulto de confianza o llama a la Línea de la Vida: 800 911 2000. <a href="/ayuda">Ver más ayuda</a>.';
        out.append(a);
      }
      out.scrollIntoView({ behavior: 'smooth', block: 'center' });
    } catch {
      out.textContent = 'No pudimos calcular tu resultado. Intenta de nuevo.';
    }
  });
})();
