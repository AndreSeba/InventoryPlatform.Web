// Recuadro para dibujar la firma con el mouse, el dedo o un lápiz (eventos de puntero). Se usa desde «Mi perfil».
// El lienzo tiene un tamaño interno fijo (se ve más chico en pantalla, nítido al imprimir) y se exporta recortado
// al trazo, como PNG con fondo transparente, para estamparlo sobre el papel.
(() => {
    const estados = new WeakMap();
    const ANCHO = 900, ALTO = 300, GROSOR = 4.5;

    function punto(canvas, e) {
        const r = canvas.getBoundingClientRect();
        return { x: (e.clientX - r.left) * (canvas.width / r.width), y: (e.clientY - r.top) * (canvas.height / r.height) };
    }

    window.firmaPad = {
        iniciar(canvas) {
            if (!canvas || estados.has(canvas)) return;
            canvas.width = ANCHO;
            canvas.height = ALTO;
            const ctx = canvas.getContext('2d');
            ctx.lineCap = 'round';
            ctx.lineJoin = 'round';
            ctx.strokeStyle = '#111';
            ctx.lineWidth = GROSOR;
            const estado = { dibujando: false, ultimo: null, trazos: 0 };
            estados.set(canvas, estado);

            canvas.addEventListener('pointerdown', e => {
                e.preventDefault();
                canvas.setPointerCapture(e.pointerId);
                estado.dibujando = true;
                estado.ultimo = punto(canvas, e);
                estado.trazos++;
                // un punto (por si solo toca)
                ctx.beginPath();
                ctx.arc(estado.ultimo.x, estado.ultimo.y, GROSOR / 2, 0, Math.PI * 2);
                ctx.fillStyle = '#111';
                ctx.fill();
            });
            canvas.addEventListener('pointermove', e => {
                if (!estado.dibujando) return;
                e.preventDefault();
                const p = punto(canvas, e);
                const medio = { x: (estado.ultimo.x + p.x) / 2, y: (estado.ultimo.y + p.y) / 2 };
                ctx.beginPath();
                ctx.moveTo(estado.ultimo.x, estado.ultimo.y);
                ctx.quadraticCurveTo(estado.ultimo.x, estado.ultimo.y, medio.x, medio.y);
                ctx.lineTo(p.x, p.y);
                ctx.stroke();
                estado.ultimo = p;
            });
            const soltar = e => { estado.dibujando = false; estado.ultimo = null; };
            canvas.addEventListener('pointerup', soltar);
            canvas.addEventListener('pointercancel', soltar);
            canvas.addEventListener('pointerleave', soltar);
        },

        limpiar(canvas) {
            if (!canvas) return;
            canvas.getContext('2d').clearRect(0, 0, canvas.width, canvas.height);
            const estado = estados.get(canvas);
            if (estado) estado.trazos = 0;
        },

        // PNG (base64, sin el prefijo data:) recortado al trazo. null si no hay trazo o es demasiado corto.
        exportar(canvas) {
            if (!canvas) return null;
            const ctx = canvas.getContext('2d');
            const { data, width, height } = ctx.getImageData(0, 0, canvas.width, canvas.height);
            let minX = width, minY = height, maxX = -1, maxY = -1;
            for (let y = 0; y < height; y++) {
                for (let x = 0; x < width; x++) {
                    if (data[(y * width + x) * 4 + 3] > 8) {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }
            if (maxX < 0 || (maxX - minX) < 40) return null;   // vacío o un simple punto

            const margen = 14;
            const ancho = Math.max(maxX - minX + 1 + margen * 2, 120);
            const alto = Math.max(maxY - minY + 1 + margen * 2, 50);
            // Se reduce a la mitad (como mucho 480 px de ancho): alcanza para imprimir una firma de ~5 cm y el PNG pesa ~4 veces menos.
            const escala = Math.min(1, 480 / ancho);
            const salida = document.createElement('canvas');
            salida.width = Math.max(60, Math.round(ancho * escala));
            salida.height = Math.max(30, Math.round(alto * escala));
            const cx = (minX + maxX) / 2, cy = (minY + maxY) / 2;
            const sctx = salida.getContext('2d');
            sctx.imageSmoothingQuality = 'high';
            sctx.drawImage(canvas, cx - ancho / 2, cy - alto / 2, ancho, alto, 0, 0, salida.width, salida.height);
            return salida.toDataURL('image/png').split(',')[1];
        },

        // Pinta una firma ya guardada (para «Dibujar de nuevo» partiendo de cero no hace falta; esto es solo la vista previa).
        hayTrazo(canvas) {
            const estado = canvas && estados.get(canvas);
            return !!(estado && estado.trazos > 0);
        },
    };
})();
