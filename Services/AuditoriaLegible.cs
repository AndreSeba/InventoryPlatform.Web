using System.Globalization;
using System.Text.Json;
using Inventory.Domain.Enums;
using Inventory.Domain.Security;

namespace Inventory.Web.Services;

// Nombres por id para que la auditoría diga «Vaso logo Mabel» y no «ProductoId 1392». Si un catálogo no se pudo
// cargar (por ejemplo, falta un permiso), ese id se muestra como «#1392».
public sealed class CatalogosAuditoria
{
    public Dictionary<int, string> Productos { get; } = [];
    public Dictionary<int, string> Ubicaciones { get; } = [];
    public Dictionary<int, string> Almacenes { get; } = [];
    public Dictionary<int, string> Areas { get; } = [];
    public Dictionary<int, string> Categorias { get; } = [];
    public Dictionary<int, string> Roles { get; } = [];
    public Dictionary<int, string> Usuarios { get; } = [];
}

public sealed record DiferenciaLegible(string Campo, IReadOnlyList<string> Antes, IReadOnlyList<string> Despues);

// Convierte el JSON que guarda la auditoría (antes / después) en texto que una persona lee: nombres de campo en
// español, estados con su nombre, ids resueltos, fechas, Sí/No y listas en líneas, mostrando solo lo que cambió.
public static class AuditoriaLegible
{
    private static readonly Dictionary<string, string> Entidades = new()
    {
        ["Producto"] = "Producto", ["Movimiento"] = "Movimiento", ["Solicitud"] = "Solicitud", ["SesionConteo"] = "Conteo físico",
        ["Usuario"] = "Usuario", ["Rol"] = "Rol", ["Categoria"] = "Categoría", ["Area"] = "Área", ["Ubicacion"] = "Ubicación",
        ["Unidad"] = "Unidad", ["Almacen"] = "Almacén", ["Pais"] = "País", ["AvisoDevolucion"] = "Aviso de devolución",
        ["RevisionAcceso"] = "Revisión de accesos",
    };

    private static readonly Dictionary<string, string> Acciones = new()
    {
        ["RegistrarEntrada"] = "Registrar entrada", ["RegistrarSalida"] = "Registrar salida",
        ["RegistrarAjustePositivo"] = "Ajuste positivo", ["RegistrarAjusteNegativo"] = "Ajuste negativo",
        ["RegistrarDevolucion"] = "Registrar devolución",
    };

    private static readonly Dictionary<string, string> Campos = new()
    {
        ["Estado"] = "Estado", ["Tipo"] = "Tipo", ["Detalles"] = "Líneas", ["Lineas"] = "Líneas", ["Codigo"] = "Código", ["Nombre"] = "Nombre",
        ["Descripcion"] = "Descripción", ["Activo"] = "Activo", ["Motivo"] = "Motivo", ["MotivoRechazo"] = "Motivo del rechazo",
        ["NumeroMovimiento"] = "N.º de movimiento", ["NumeroSolicitud"] = "N.º de solicitud", ["ProductoId"] = "Producto",
        ["TipoMovimiento"] = "Tipo de movimiento", ["Cantidad"] = "Cantidad", ["UbicacionId"] = "Ubicación", ["Retorna"] = "Es préstamo",
        ["UbicacionExterna"] = "Destino del préstamo", ["FechaRetornoEsperada"] = "Retorno esperado", ["FechaVencimiento"] = "Vencimiento",
        ["MovimientoOrigenId"] = "Movimiento de origen", ["SolicitudDetalleId"] = "Línea de solicitud", ["AreaId"] = "Área",
        ["ClaveProducto"] = "Clave", ["CodigoProducto"] = "Código", ["CategoriaId"] = "Categoría", ["UnidadMedida"] = "Unidad",
        ["CostoUnitario"] = "Costo unitario", ["StockMinimo"] = "Stock mínimo", ["Detalle"] = "Detalle", ["TieneImagen"] = "Tiene foto",
        ["Email"] = "Correo", ["NombreCompleto"] = "Nombre", ["RolId"] = "Rol", ["Permisos"] = "Permisos",
        ["CodigoAlmacen"] = "Código", ["TipoAlmacen"] = "Tipo de almacén", ["ProveedorNombre"] = "Proveedor", ["ProveedorContacto"] = "Contacto del proveedor",
        ["ProveedorDireccion"] = "Dirección del proveedor", ["CodigoUbicacion"] = "Código", ["TipoUbicacion"] = "Tipo de ubicación", ["Nro"] = "N.º",
        ["Lado"] = "Lado", ["Nivel"] = "Nivel", ["AlmacenId"] = "Almacén", ["CodigoArea"] = "Código", ["NombreArea"] = "Nombre",
        ["CodigoCategoria"] = "Código", ["EncargadoId"] = "Encargado", ["CodigoUnidad"] = "Código", ["CodigoIso"] = "Código ISO",
        ["MovimientoOrigen"] = "Movimiento de origen", ["Notas"] = "Notas", ["CantidadRecibida"] = "Cantidad recibida", ["Entrada"] = "Entrada creada",
        ["ConteoOrigenId"] = "Conteo de origen", ["LineasConDiferencia"] = "Líneas con diferencia", ["Evidencias"] = "Archivos de evidencia",
        ["NombreArchivo"] = "Archivo", ["ContentType"] = "Tipo de archivo", ["TamanoBytes"] = "Tamaño", ["Alcance"] = "Alcance", ["Cuentas"] = "Cuentas",
        ["Mantener"] = "Mantener", ["Quitar"] = "Quitar acceso", ["CambiarRol"] = "Cambiar de rol", ["Decision"] = "Decisión",
        ["RolNuevo"] = "Rol nuevo", ["Comentario"] = "Comentario", ["UsuarioEmail"] = "Cuenta",
    };

    // Los campos de un renglón de una lista (líneas de solicitud, etc.)
    private static readonly Dictionary<string, string> CamposDeLinea = new()
    {
        ["CantidadSolicitada"] = "pidió", ["CantidadAprobada"] = "aprobada", ["CantidadEntregada"] = "entregada",
    };

    public static string Entidad(string entidad) => Entidades.GetValueOrDefault(entidad) ?? Separar(entidad);
    public static string Accion(string accion) => Acciones.GetValueOrDefault(accion) ?? Separar(accion);

    private static string Separar(string t)
    {
        var s = System.Text.RegularExpressions.Regex.Replace(t, "(?<=[a-z])(?=[A-Z])", " ");
        return s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant();
    }

    // ---------------------------------------------------------------- diferencias
    public static List<DiferenciaLegible> Diferencias(string entidad, string? anteriorJson, string? nuevoJson, CatalogosAuditoria cat)
    {
        var anterior = Objeto(anteriorJson);
        var nuevo = Objeto(nuevoJson);
        var nombres = new List<string>();
        if (anterior is not null) nombres.AddRange(anterior.Value.EnumerateObject().Select(p => p.Name));
        if (nuevo is not null) nombres.AddRange(nuevo.Value.EnumerateObject().Select(p => p.Name));

        var resultado = new List<DiferenciaLegible>();
        foreach (var campo in nombres.Distinct())
        {
            JsonElement? a = Propiedad(anterior, campo), n = Propiedad(nuevo, campo);

            if ((a?.ValueKind == JsonValueKind.Array) || (n?.ValueKind == JsonValueKind.Array))
            {
                var (antes, despues) = Lista(entidad, campo, a, n, cat);
                if (antes.Count > 0 || despues.Count > 0)
                    resultado.Add(new DiferenciaLegible(Etiqueta(campo), antes.Count == 0 ? ["—"] : antes, despues.Count == 0 ? ["—"] : despues));
                continue;
            }

            var ta = Valor(entidad, campo, a, cat);
            var tn = Valor(entidad, campo, n, cat);
            if (ta != tn)
                resultado.Add(new DiferenciaLegible(Etiqueta(campo), [ta], [tn]));
        }
        return resultado;
    }

    // Un nombre de campo fuera del catálogo se separa solo si es un identificador (PascalCase); los que son datos
    // («CODIGO @ UBICACION» del conteo) se muestran tal cual.
    private static string Etiqueta(string campo) =>
        Campos.GetValueOrDefault(campo) ?? (campo.All(char.IsLetter) ? Separar(campo) : campo);

    // ---------------------------------------------------------------- valores simples
    private static string Valor(string entidad, string campo, JsonElement? v, CatalogosAuditoria cat)
    {
        if (v is null || v.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return "—";
        var e = v.Value;

        switch (e.ValueKind)
        {
            case JsonValueKind.True: return "Sí";
            case JsonValueKind.False: return "No";
            case JsonValueKind.String:
                var s = e.GetString() ?? "";
                if (s.Length == 0) return "—";
                if (campo.StartsWith("Fecha") && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var f)) return f.ToString("dd/MM/yyyy");
                return s;
            case JsonValueKind.Number:
                if (!e.TryGetInt64(out var num)) return e.ToString();
                return Numero(entidad, campo, (int)num, num, cat);
            case JsonValueKind.Object:
                return string.Join(" · ", e.EnumerateObject().Select(p => $"{Etiqueta(p.Name)}: {Valor(entidad, p.Name, p.Value, cat)}"));
            default:
                return e.GetRawText();
        }
    }

    private static string Numero(string entidad, string campo, int n, long largo, CatalogosAuditoria cat)
    {
        string Nombre(Dictionary<int, string> d) => d.TryGetValue(n, out var t) ? t : $"#{n}";
        string Enum<T>() where T : struct, Enum => System.Enum.IsDefined(typeof(T), n) ? Separar(((T)(object)n).ToString()!) : n.ToString();

        return (entidad, campo) switch
        {
            (_, "ProductoId") => Nombre(cat.Productos),
            (_, "UbicacionId") => Nombre(cat.Ubicaciones),
            (_, "AlmacenId") => Nombre(cat.Almacenes),
            (_, "AreaId") => Nombre(cat.Areas),
            (_, "CategoriaId") => Nombre(cat.Categorias),
            (_, "RolId") => Nombre(cat.Roles),
            (_, "EncargadoId") => Nombre(cat.Usuarios),
            (_, "MovimientoOrigenId") => $"movimiento #{n}",
            (_, "SolicitudDetalleId") => $"línea #{n}",
            (_, "ConteoOrigenId") => $"conteo #{n}",
            ("Solicitud", "Estado") => Estado(n, typeof(EstadoSolicitud)),
            ("Solicitud", "Tipo") => Enum<TipoSolicitud>(),
            ("AvisoDevolucion", "Estado") => Estado(n, typeof(EstadoAvisoDevolucion)),
            ("SesionConteo", "Estado") => Estado(n, typeof(EstadoConteo)),
            ("RevisionAcceso", "Estado") => Estado(n, typeof(EstadoRevisionAcceso)),
            (_, "TipoMovimiento") => Enum<TipoMovimiento>(),
            (_, "TipoUbicacion") => Enum<TipoUbicacion>(),
            (_, "TipoAlmacen") => Enum<TipoAlmacen>(),
            (_, "Alcance") => n == 1 ? "Todas las cuentas" : n == 2 ? "Administradores" : n.ToString(),
            (_, "Decision") => n switch { 0 => "Pendiente", 1 => "Mantener", 2 => "Quitar acceso", 3 => "Cambiar de rol", _ => n.ToString() },
            (_, "TamanoBytes") => largo >= 1048576 ? $"{largo / 1048576.0:0.0} MB" : $"{Math.Max(1, largo / 1024)} KB",
            (_, "CostoUnitario") => largo.ToString("N2", CultureInfo.InvariantCulture),
            _ => largo.ToString("N0", new CultureInfo("es-BO")),
        };
    }

    private static string Estado(int n, Type tipo) => System.Enum.IsDefined(tipo, n) ? Separar(System.Enum.ToObject(tipo, n).ToString()!) : n.ToString();

    // ---------------------------------------------------------------- listas
    private static (List<string> Antes, List<string> Despues) Lista(string entidad, string campo, JsonElement? a, JsonElement? n, CatalogosAuditoria cat)
    {
        var la = a?.ValueKind == JsonValueKind.Array ? a.Value.EnumerateArray().ToList() : [];
        var ln = n?.ValueKind == JsonValueKind.Array ? n.Value.EnumerateArray().ToList() : [];

        // Lista de números (los permisos de un rol): se muestran los códigos y solo lo que se agregó o se quitó.
        if (campo == "Permisos" || la.Concat(ln).All(x => x.ValueKind == JsonValueKind.Number))
        {
            var ida = la.Where(x => x.ValueKind == JsonValueKind.Number).Select(x => x.GetInt32()).ToHashSet();
            var idn = ln.Where(x => x.ValueKind == JsonValueKind.Number).Select(x => x.GetInt32()).ToHashSet();
            string Cod(int id) => campo == "Permisos" && id >= 1 && id <= Permisos.Catalogo.Count ? Permisos.Catalogo[id - 1].Codigo : $"#{id}";
            if (a is null)
                return ([], [$"{idn.Count} permiso(s)"]);
            var agregados = idn.Except(ida).OrderBy(x => x).Select(Cod).ToList();
            var quitados = ida.Except(idn).OrderBy(x => x).Select(Cod).ToList();
            var antes = new List<string>(); var despues = new List<string>();
            quitados.ForEach(q => antes.Add($"− {q}"));
            agregados.ForEach(g => despues.Add($"+ {g}"));
            return (antes, despues);
        }

        // Lista de renglones (líneas de una solicitud): se emparejan por Id (o por producto) y solo salen los que cambiaron.
        string? Clave(JsonElement x) => x.ValueKind != JsonValueKind.Object ? null
            : x.TryGetProperty("Id", out var id) ? $"id:{id}"
            : x.TryGetProperty("ProductoId", out var p) ? $"prod:{p}" : null;
        string Linea(JsonElement x) => x.ValueKind != JsonValueKind.Object ? Valor(entidad, campo, x, cat) : LineaTexto(entidad, x, cat);

        var ma = la.Select((x, i) => (Clave: Clave(x) ?? $"i:{i}", Elemento: x)).ToDictionary(t => t.Clave, t => t.Elemento);
        var mn = ln.Select((x, i) => (Clave: Clave(x) ?? $"i:{i}", Elemento: x)).ToDictionary(t => t.Clave, t => t.Elemento);
        var A = new List<string>(); var N = new List<string>();
        foreach (var k in ma.Keys.Concat(mn.Keys).Distinct())
        {
            var tieneA = ma.TryGetValue(k, out var ea); var tieneN = mn.TryGetValue(k, out var en);
            if (tieneA && tieneN && ea.ValueKind == JsonValueKind.Object && en.ValueKind == JsonValueKind.Object)
            {
                // El mismo renglón en los dos lados: solo lo que cambió dentro de él («aprobada — → 60»).
                var (ta, tn) = (LineaTexto(entidad, ea, cat, en), LineaTexto(entidad, en, cat, ea));
                if (ta != tn) { A.Add(ta); N.Add(tn); }
                continue;
            }
            var textoA = tieneA ? Linea(ea) : null; var textoN = tieneN ? Linea(en) : null;
            if (textoA == textoN) continue;
            if (textoA is not null) A.Add(textoA);
            if (textoN is not null) N.Add(textoN);
        }
        return (A, N);
    }

    // «otro»: el mismo renglón del otro lado; si viene, solo se escriben los campos que difieren.
    private static string LineaTexto(string entidad, JsonElement x, CatalogosAuditoria cat, JsonElement? otro = null)
    {
        string V(string campo) => Valor(entidad, campo, x.TryGetProperty(campo, out var v) ? v : null, cat);
        bool Tiene(string campo) => x.TryGetProperty(campo, out _);
        bool Cambio(string campo) => otro is null || V(campo) != Valor(entidad, campo, otro.Value.TryGetProperty(campo, out var o) ? o : null, cat);

        var partes = new List<string>();
        foreach (var (campo, etiqueta) in CamposDeLinea)
            if (Tiene(campo) && Cambio(campo)) partes.Add($"{etiqueta} {V(campo)}");
        if (otro is null && Tiene("Retorna") && x.GetProperty("Retorna").ValueKind == JsonValueKind.True) partes.Add("préstamo");

        var cabecera = Tiene("ProductoId") ? V("ProductoId") : Tiene("Id") ? $"Línea #{x.GetProperty("Id")}" : "Línea";
        return partes.Count == 0 ? cabecera : $"{cabecera}: {string.Join(" · ", partes)}";
    }

    // ---------------------------------------------------------------- JSON
    private static JsonElement? Objeto(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var e = JsonDocument.Parse(json).RootElement;
            return e.ValueKind == JsonValueKind.Object ? e : null;
        }
        catch (JsonException) { return null; }
    }

    private static JsonElement? Propiedad(JsonElement? c, string campo) =>
        c is not null && c.Value.TryGetProperty(campo, out var v) ? v : null;
}
