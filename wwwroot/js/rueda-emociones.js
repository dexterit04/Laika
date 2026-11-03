(async () => {
  const svg = document.getElementById('rueda');
  const caja = document.getElementById('consejo');
  if (!svg) return;
  const NS = 'http://www.w3.org/2000/svg';
  const emociones = await (await fetch('/api/emociones')).json();
  const n = emociones.length, R = 100, r = 32;
  const punto = (ang, rad) => [Math.cos(ang) * rad, Math.sin(ang) * rad];

  emociones.forEach((em, i) => {
    const a0 = (i / n) * 2 * Math.PI - Math.PI / 2;
    const a1 = ((i + 1) / n) * 2 * Math.PI - Math.PI / 2;
    const [x0, y0] = punto(a0, R), [x1, y1] = punto(a1, R);
    const [x2, y2] = punto(a1, r), [x3, y3] = punto(a0, r);
    const path = document.createElementNS(NS, 'path');
    path.setAttribute('d', `M${x0},${y0} A${R},${R} 0 0 1 ${x1},${y1} L${x2},${y2} A${r},${r} 0 0 0 ${x3},${y3} Z`);
    path.setAttribute('fill', em.color);
    path.setAttribute('tabindex', '0');
    path.setAttribute('role', 'button');
    path.setAttribute('aria-label', em.nombre);

    const elegir = () => {
      svg.querySelectorAll('path').forEach(p => p.classList.remove('sel'));
      path.classList.add('sel');
      caja.replaceChildren();
      const h = document.createElement('h3'); h.textContent = em.nombre;
      const d = document.createElement('p'); d.textContent = em.descripcion;
      const c = document.createElement('p'); c.textContent = em.consejo;
      caja.append(h, d, c);
    };
    path.addEventListener('click', elegir);
    path.addEventListener('keydown', e => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); elegir(); } });
    svg.appendChild(path);

    const mid = (a0 + a1) / 2;
    const [tx, ty] = punto(mid, (R + r) / 2);
    const t = document.createElementNS(NS, 'text');
    t.setAttribute('x', tx); t.setAttribute('y', ty + 3);
    t.textContent = em.nombre;
    svg.appendChild(t);
  });
})();
