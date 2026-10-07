namespace Inventory.Web.Services;

// Interruptores de módulos ya construidos que todavía no se habilitaron (sección «Funciones» de appsettings).
// Apagado = el módulo aparece como «Planificado» y no muestra avisos ni pantallas funcionales.
public class FuncionesOptions
{
    public const string SectionName = "Funciones";

    // Revisión periódica de accesos (/accesos, aviso en Inicio). Apagada hasta que se confirme su puesta en marcha.
    public bool RevisionAccesos { get; set; }
}
