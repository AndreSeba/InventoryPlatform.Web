# InventoryPlatform.Web — CLAUDE.md

> Frontend de la plataforma de Inventario de Material Promocional (Marketing,
> Nestlé Bolivia). Lee este archivo antes de tocar código. Si el código pide
> una decisión no documentada acá, preguntá antes de asumir.

## Propósito

Interfaz Blazor para `../InventoryPlatform` (el backend, repo Git
**separado** a propósito — pedido explícito del usuario para no sumar peso
al backend). Este repo **nunca** toca la base de datos ni referencia
`Inventory.Infrastructure` — todo pasa por HTTP contra la Web API.

## Relación con el backend — no tocar sin avisar

- `Inventory.Web.csproj` referencia por **path relativo**
  `../InventoryPlatform/src/Inventory.Application/Inventory.Application.csproj`
  — así los DTOs (`Inventory.Application.Dtos`) y los enums
  (`Inventory.Domain.Enums`) son los reales del backend, nunca una copia a
  mano. Si un contrato cambia en el backend, este proyecto lo ve directo al
  recompilar — no hay paso de sincronización manual.
- **Nunca edites el backend desde acá** salvo que el usuario lo pida
  explícitamente — la única vez que hizo falta fue actualizar
  `CorsOrigenesPermitidos` en `InventoryPlatform/src/Inventory.Api/appsettings.json`
  para que la API acepte los puertos de este proyecto (ver más abajo).
- `appsettings.json` → `ApiBaseUrl` apunta al puerto HTTPS de la API
  (`https://localhost:7272/` en dev). Si cambia el puerto de la API (nueva
  máquina, nuevo `dotnet new`), actualizar acá.
- **Si cambian los puertos de ESTE proyecto** (`Properties/launchSettings.json`),
  hay que avisar para actualizar `CorsOrigenesPermitidos` del lado del
  backend con los puertos nuevos (http y https) — si no, el navegador
  bloquea las respuestas aunque el request llegue bien.

## Stack

- .NET 10, Blazor Web App, **Interactive Server** (no WebAssembly, no auth
  todavía — mismas decisiones que el backend).
- Sin base de datos ni Infrastructure propios — solo `HttpClient` tipados en
  `Services/` (`ProductoApiClient`, `MovimientoApiClient`,
  `CategoriaApiClient`, `AreaApiClient`, `UbicacionApiClient`,
  `SolicitudApiClient`, `ConteoApiClient`), cada uno con el mismo patrón:
  método por endpoint, `ApiClientHelper.LanzarSiHayErrorAsync` traduce un
  `ProblemDetails` no-2xx en una `ApiException` que los componentes atrapan
  para mostrar el mensaje real sin crashear.

## Sistema de diseño — "Modernist" (cream/brown)

Adaptado de un mockup de referencia (`Inventario Marketing - Standalone.html`,
un bundle de canvas de diseño) que el usuario pidió replicar "adaptado a lo
que ya tenemos, no cambies nada del back". Todo vive en
`wwwroot/app.css` como tokens CSS + clases de componente, sin build step
(sin Tailwind, sin Sass):

- Paleta: `--color-bg` blanco, `--color-surface` crema (#f3ead8),
  `--color-accent` marrón (#6b4226), radios en 0 (esquinas rectas a
  propósito — "Modernist").
- Tipografía: **Archivo** (Google Fonts, cargada en `Components/App.razor`
  vía `<link>`, weights 400/600/800), `--font-heading-weight: 800`.
- Componentes reutilizables: `.btn` (`-primary`/`-secondary`/`-ghost`),
  `.input`, `.seg` (segmented control, usado en ¿Retorna? Sí/No y en
  Tipo de ubicación Rack/Mueble), `.card`, `.tag`
  (`-accent`/`-neutral`/`-outline`/`-warn`), `.table`, `.dialog-*`.
- Layout: `Components/Layout/WelcomeLayout.razor` (pantalla de bienvenida,
  sin sidebar — solo la ruta `/`) vs. `Components/Layout/MainLayout.razor`
  (rail lateral de íconos + topbar + `.app-main`, layout por defecto para
  todo lo demás). El rail resalta el link activo comparando
  `NavigationManager.Uri` contra el prefijo de cada ruta.
- `Components/Shared/Icon.razor`: íconos de línea inline (stroke=
  currentColor, viewBox 24x24, mismo estilo Lucide que el mockup) por
  `Name` — agregar un ícono nuevo ahí, no traer una librería de íconos.
- `Components/Shared/Dialog.razor`: wrapper de `.dialog-backdrop`/`.dialog`
  con `RenderFragment Body` y `Actions` — usarlo en vez de repetir el
  markup del modal en cada página.

## Adaptaciones del mockup al modelo real (deliberadas, no bugs)

El mockup de referencia estaba diseñado sobre una versión anterior del
modelo (v3, con `Rack` y sin `Area`/`Solicitud`/`Conteo`). Lo que cambió al
adaptarlo al backend real (`InventoryPlatform`, modelo v4):

- **"Rack" → "Ubicación"** en todo el diseño — el backend generaliza a
  `Ubicacion` (Rack o Mueble), no solo racks.
- **Sin campo "Solicitante" libre en los diálogos de movimiento** — el
  mockup tenía un input de texto suelto; el modelo real resuelve "quién
  pidió esto" a través del flujo `Solicitud`/`SolicitudDetalle` por Área, no
  como texto libre en cada movimiento.
- **KPI "Próximos a vencer" → "Préstamos pendientes"** — el mockup asumía
  `FechaVencimiento` por producto, que el backend real no tiene. Se
  reemplazó por un KPI que sí existe de verdad
  (`MovimientoApiClient.ListarPrestamosPendientesAsync`).
- **Categorías/Ubicaciones/Áreas sin Editar/Eliminar** — el mockup tenía esos
  botones; el backend solo expone `Listar`+`Crear` para esas tres entidades
  (ver `InventoryPlatform/CLAUDE.md`, sección "Operaciones que el backend no
  expone todavía"). No agregar los botones acá hasta que el backend tenga
  el endpoint.
- **Pantallas nuevas que el mockup no tenía**: `/solicitudes` (crear,
  aprobar, rechazar, entregar) y `/conteos` (conteo físico por sesión,
  comparado contra la existencia calculada) — porque el modelo real sí
  tiene esas entidades, aunque el mockup viejo no las contemplara.
- **Diálogo "Registrar ajuste"** (`AjustePositivo`/`AjusteNegativo`) — el
  mockup solo tenía Entrada/Salida; el backend real soporta 4 tipos de
  movimiento, así que se agregó un tercer diálogo con el mismo lenguaje
  visual (segmented control Positivo/Negativo).

## Rutas

| Ruta | Página | Notas |
|---|---|---|
| `/` | Bienvenida | `WelcomeLayout`, sin sidebar |
| `/inicio` | Dashboard | KPIs (Bajo mínimo, Préstamos pendientes) + movimientos recientes |
| `/productos` | Catálogo | Grilla con búsqueda + chips de categoría, diálogo crear/editar |
| `/productos/{id}` | Detalle | Historial de movimientos + ficha imprimible (`@media print` oculta el rail/topbar) |
| `/movimientos` | Movimientos | Diálogos Entrada/Salida/Ajuste + tabla filtrable + sección de préstamos pendientes con devolución |
| `/solicitudes` | Solicitudes | Listado filtrable por estado + diálogo de creación con líneas dinámicas |
| `/solicitudes/{id}` | Detalle de solicitud | Aprobar (por línea)/Rechazar (Pendiente) o Entregar (Aprobada/EntregadaParcial → crea una Salida real) |
| `/conteos` | Conteo físico | Por sesión: registrar conteo + comparación contra existencia del sistema |
| `/categorias`, `/areas`, `/ubicaciones` | Catálogos simples | Tabla + diálogo de alta. Sin editar/eliminar (ver arriba) |

## Comandos

```bash
dotnet build
dotnet run --launch-profile https      # https://localhost:7126
```

**Necesita el backend corriendo en paralelo** (`InventoryPlatform/src/Inventory.Api`,
`https://localhost:7272`) — sin eso, todas las páginas muestran el error
genérico controlado ("Ocurrió un error inesperado") en vez de datos, que es
el comportamiento esperado y ya verificado, no un bug.

## Lo que NO hacer

- ❌ No dupliques DTOs a mano — vienen de `Inventory.Application` por
  referencia de proyecto.
- ❌ No agregues acceso a datos ni `Inventory.Infrastructure` acá — todo pasa
  por los `*ApiClient` de `Services/`.
- ❌ No agregues botones de Editar/Eliminar a Categorías/Ubicaciones/Áreas
  hasta que el backend exponga esos endpoints.
- ❌ No uses comillas escapadas (`\"`) dentro de un atributo Razor de
  comillas dobles para meter una expresión C# con comillas — Razor no las
  interpreta como escape de HTML y rompe la compilación (`RZ`/`CS1012`).
  Patrón seguro: extraer un método (`private void Ir(int id) =>
  Nav.NavigateTo($"/ruta/{id}");`) y usarlo en el `@onclick`, en vez de
  interpolar el string directo en el atributo.
- ❌ No compares un `string` contra un literal de comillas simples dentro de
  un atributo Razor de comillas dobles (`Visible="_dialog == 'x'"`) — C# lo
  interpreta como `char` y explota si el literal tiene más de un carácter.
  Usar `Visible="@(_dialog == "x")"` con el atributo en comillas dobles y
  el string C# también en dobles, o cambiar el delimitador del atributo a
  comillas simples.
- ❌ No escribas `@{ ... }` para abrir un bloque de código Razor
  inmediatamente después de markup dentro de un `else { }` ya en contexto
  de código — Razor tira `RZ1010` ("ya estás en un bloque de código"). Si
  hace falta una variable intermedia, o se calcula más arriba (antes del
  `@if`) o se llama al método directo en cada lugar donde se usa.
