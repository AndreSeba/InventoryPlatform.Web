// Tema claro/oscuro — persistido en localStorage, aplicado como atributo
// data-theme en <html> (ver tokens [data-theme="dark"] en app.css).
window.tema = {
    get: function () {
        try { return localStorage.getItem("tema") || "light"; } catch { return "light"; }
    },
    set: function (valor) {
        try { localStorage.setItem("tema", valor); } catch { }
        document.documentElement.setAttribute("data-theme", valor);
    }
};
