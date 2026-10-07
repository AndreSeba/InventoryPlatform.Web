function base64ABytes(base64) {
    const bytes = atob(base64);
    const buffer = new Uint8Array(bytes.length);
    for (let i = 0; i < bytes.length; i++) buffer[i] = bytes.charCodeAt(i);
    return buffer;
}

window.descargarArchivo = (nombreArchivo, contentType, base64) => {
    const blob = new Blob([base64ABytes(base64)], { type: contentType });
    const url = URL.createObjectURL(blob);
    const enlace = document.createElement('a');
    enlace.href = url;
    enlace.download = nombreArchivo;
    document.body.appendChild(enlace);
    enlace.click();
    document.body.removeChild(enlace);
    URL.revokeObjectURL(url);
};

// URL temporal para mostrar un archivo (imagen/PDF) dentro de la página. Quien la pide la
// libera con liberarUrlBlob al cerrar el visor.
window.crearUrlBlob = (contentType, base64) =>
    URL.createObjectURL(new Blob([base64ABytes(base64)], { type: contentType }));

window.liberarUrlBlob = (url) => URL.revokeObjectURL(url);

// Abre el diálogo de impresión SIN bloquear la llamada de Blazor: window.print() no vuelve hasta que se cierra el diálogo,
// y si tarda más del tiempo de espera del circuito (1 min) la llamada falla con TaskCanceledException.
window.imprimirPagina = () => { setTimeout(() => window.print(), 0); };
