// Tema claro/oscuro — persistido en localStorage, aplicado como atributo
// data-theme en <html> (ver tokens [data-theme="dark"] en app.css).
window.tema = {
    get: function () {
        // Primero lo que ya está aplicado en la página (el login puede arrancar oscuro sin
        // haber guardado nada); si no, lo guardado.
        if (document.documentElement.getAttribute("data-theme") === "dark") return "dark";
        try { return localStorage.getItem("tema") || "light"; } catch { return "light"; }
    },
    set: function (valor) {
        try { localStorage.setItem("tema", valor); } catch { }
        document.documentElement.setAttribute("data-theme", valor);
    }
};
