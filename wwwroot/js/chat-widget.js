(() => {
  const fab = document.getElementById('chatFab');
  const panel = document.getElementById('chatPanel');
  const log = document.getElementById('chatLog');
  const input = document.getElementById('chatInput');
  const send = document.getElementById('chatSend');
  const close = document.getElementById('chatClose');
  if (!fab) return;

  // La conversación se guarda solo en esta pestaña (sessionStorage) para que no se pierda al cambiar de página.
  let estado = { perfil: null, historial: [], ui: [] };
  try { estado = JSON.parse(sessionStorage.getItem('laikaChat')) || estado; } catch { }
  const guardar = () => { try { sessionStorage.setItem('laikaChat', JSON.stringify(estado)); } catch { } };

  function abrir(v) {
    panel.hidden = !v;
    fab.setAttribute('aria-expanded', v);
    if (v) input.focus();
  }
  fab.addEventListener('click', () => abrir(panel.hidden));
  close.addEventListener('click', () => { abrir(false); fab.focus(); });

  function burbuja(texto, clase, acciones, urgente) {
    const d = document.createElement('div');
    d.className = 'msg ' + clase + (urgente ? ' urgente' : '');
    const t = document.createElement('span');
    t.textContent = texto;
    d.appendChild(t);
    if (acciones && acciones.length) {
      const c = document.createElement('div');
      c.className = 'acciones';
      acciones.forEach(a => {
        const l = document.createElement(a.url ? 'a' : 'button');
        l.className = 'chip';
        l.textContent = a.texto;
        if (a.url) l.href = a.url; else { l.type = 'button'; l.addEventListener('click', a.onclick); }
        c.appendChild(l);
      });
      d.appendChild(c);
    }
    log.appendChild(d);
    log.scrollTop = log.scrollHeight;
    return d;
  }

  function elegirPerfil(p) {
    estado.perfil = p;
    estado.ui.push({ t: p === 'docente' ? 'Soy docente' : 'Soy estudiante', c: 'yo' });
    estado.ui.push({ t: 'Gracias por contármelo. Aquí no necesito tu nombre ni datos. Cuéntame, ¿qué tienes en mente hoy?', c: 'bot' });
    guardar();
    pintar();
  }

  function pintar() {
    log.replaceChildren();
    if (!estado.ui.length) {
      burbuja('Hola, soy Laika. Este es un espacio seguro y anónimo. Para acompañarte mejor, ¿quién eres?', 'bot', [
        { texto: 'Soy estudiante', onclick: () => elegirPerfil('estudiante') },
        { texto: 'Soy docente', onclick: () => elegirPerfil('docente') },
        { texto: 'Prefiero no decirlo', onclick: () => { estado.ui.push({ t: 'Hola, soy Laika. Cuéntame cómo te sientes hoy.', c: 'bot' }); guardar(); pintar(); } }
      ]);
      return;
    }
    estado.ui.forEach(m => burbuja(m.t, m.c, m.a, m.u));
  }

  async function enviar() {
    const texto = input.value.trim();
    if (!texto) return;
    input.value = '';
    burbuja(texto, 'yo');
    estado.ui.push({ t: texto, c: 'yo' });
    estado.historial.push({ rol: 'user', contenido: texto });
    const espera = burbuja('Escribiendo...', 'bot');
    send.disabled = true;
    try {
      const res = await fetch('/api/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mensajes: estado.historial, perfil: estado.perfil, pagina: location.pathname })
      });
      if (!res.ok) throw new Error();
      const d = await res.json();
      const acciones = (d.acciones || []).map(a => ({ texto: a.texto, url: a.url }));
      espera.remove();
      burbuja(d.contenido, 'bot', acciones, d.urgencia);
      estado.historial.push({ rol: 'assistant', contenido: d.contenido });
      estado.ui.push({ t: d.contenido, c: 'bot', a: acciones, u: d.urgencia });
    } catch {
      espera.remove();
      burbuja('No pude conectarme. Si necesitas ayuda ahora, llama al 911.', 'bot');
    } finally {
      send.disabled = false;
      guardar();
    }
  }
  send.addEventListener('click', enviar);
  input.addEventListener('keydown', e => { if (e.key === 'Enter') enviar(); });
  pintar();
})();
