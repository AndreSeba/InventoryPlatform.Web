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
- **`Inventory.Web.slnx` incluye los proyectos del backend a propósito**
  (`Inventory.Application` + `Inventory.Domain`, por ruta relativa, en la
  carpeta virtual `backend/`). Sin eso, abrir el `.csproj` suelto en Visual
  Studio falla al hacer restore con *"Unable to find project information for
  '...Inventory.Application.csproj'. ... the project is unloaded or not part
  of the current solution"* al dar F5: VS solo restaura los proyectos que
  están **dentro de la solución**, y la `ProjectReference` apunta afuera
  (`dotnet build` por consola sí funciona, porque el CLI recorre las
  referencias sin depender de la solución). Los dos proyectos del backend se
  agregan como referencia, no se copian — siguen viviendo en su repo. Si
  algún día hace falta levantar API y front juntos desde acá, se puede sumar
  `Inventory.Api` a la misma solución y usar varios proyectos de inicio; hoy
  no está, para no arrastrar `Inventory.Infrastructure` a este repo.

## Stack

- .NET 10, Blazor Web App, **Interactive Server** (no WebAssembly).
- Sin base de datos ni Infrastructure propios — solo `HttpClient` tipados en
  `Services/` (`ProductoApiClient`, `MovimientoApiClient`,
  `CategoriaApiClient`, `AreaApiClient`, `UbicacionApiClient`,
  `SolicitudApiClient`, `ConteoApiClient`, `UsuarioApiClient`, `RolApiClient`,
  `UnidadApiClient`),
  cada uno con el mismo patrón: método por endpoint,
  `ApiClientHelper.LanzarSiHayErrorAsync` traduce un `ProblemDetails` no-2xx
  (o un 401/403 sin cuerpo del handler de JWT) en una `ApiException` que los
  componentes atrapan para mostrar el mensaje real sin crashear.
- Auth: JWT contra la API — ver "Autenticación y permisos" más abajo.

## Sistema de diseño — "Corporate" (sobrio, neutro)

**Reemplaza al "Modernist" cream/brown original** — pedido explícito del
usuario el 2026-09-13 ("cambia el diseño a uno más sobrio y corporativo").
El mockup de referencia (`Inventario Marketing - Standalone.html`) sigue
siendo la base de la *estructura* (rail lateral, cards, tablas, diálogos),
pero la paleta y la tipografía cambiaron por completo. Todo vive en
`wwwroot/app.css` como tokens CSS + clases de componente, sin build step
(sin Tailwind, sin Sass) — **los nombres de variable no cambiaron** (sigue
siendo `--color-bg`, `--color-accent`, `--radius-md`, etc.), solo sus
valores, así que ningún archivo `.razor` tuvo que tocarse para el cambio de
paleta en sí — el `app.css` se reescribió completo y listo.

- Paleta: `--color-bg` gris muy claro (#f4f5f7), `--color-surface` blanco,
  `--color-accent` azul corporativo (#1d4ed8), `--radius-md: 6px` (esquinas
  suaves, ya no rectas a 0 como el Modernist).
- Tipografía: **Inter** (Google Fonts, cargada en `Components/App.razor` vía
  `<link>`, weights 400/500/600/700), `--font-heading-weight: 600` (ya no
  800 — títulos más discretos).
- **El rail lateral (sidebar) usa tokens propios**, desacoplados de
  `--color-text`: `--sidebar-bg` (#111827, oscuro) + `--sidebar-text`/
  `--sidebar-active-bg`/etc. — a propósito, porque el rail es oscuro aunque
  el resto de la app sea claro; si se necesita un tema oscuro completo algún
  día, esos tokens del sidebar ya están separados de los del contenido.
- El rail pasó de 84px icono-solo a 220px con ícono + etiqueta completa
  ("Movimientos", no "Movim.") — más legible, estilo panel admin corporativo
  típico, y con **secciones** ("Catálogos", "Administración") que separan
  los grupos de links.
- Componentes reutilizables sin cambios de nombre: `.btn`
  (`-primary`/`-secondary`/`-ghost`), `.input`, `.seg`, `.card`, `.tag`
  (`-accent`/`-neutral`/`-outline`/`-warn`), `.table`, `.dialog-*`.
- Colores "de alarma" (bajo stock, error) ya **no** usan
  `var(--color-accent-700)`/`-800` como en el Modernist (ahí esos tokens
  eran un marrón-rojizo que leía como advertencia; ahora son azul oscuro,
  que no lee como alarma) — se usa un rojo hardcodeado `#b42318` puntual en
  esos lugares (`Productos/Detalle.razor`, botón Rechazar de
  `Solicitudes/Detalle.razor`). Si se agrega un nuevo indicador de alarma,
  usar ese mismo rojo, no `--color-accent-700`.
- `Components/Layout/WelcomeLayout.razor`: layout sin sidebar, usado solo
  por `/` (ahora la pantalla de **login real**, no una bienvenida
  decorativa — ver "Autenticación y permisos"). `Components/Layout/MainLayout.razor`
  es el layout por defecto para todo lo demás: rail lateral + topbar +
  `.app-main`, **y además el punto único donde se exige sesión activa** (ver
  abajo). El rail resalta el link activo comparando `NavigationManager.Uri`
  contra el prefijo de cada ruta.
- `Components/Shared/Icon.razor`: íconos de línea inline (stroke=
  currentColor, viewBox 24x24, estilo Lucide) por `Name` — agregar un ícono
  nuevo ahí, no traer una librería de íconos. Tiene `users` y `shield`
  agregados para Usuarios/Roles.
- `Components/Shared/Dialog.razor`: wrapper de `.dialog-backdrop`/`.dialog`
  con `RenderFragment Body` y `Actions` — usarlo en vez de repetir el
  markup del modal en cada página.

## Autenticación y permisos (agregado 2026-09-13, pedido explícito del usuario)

**Sin `AuthenticationStateProvider`/`ClaimsPrincipal`/`[Authorize]` de Blazor
a propósito** — se implementó un mecanismo propio, más simple, porque la
autorización *real* ya la exige la API (JWT + policy por permiso); lo que
hace falta acá es solo UX (esconder botones que el usuario no podría usar
igual) + un gate de "¿hay sesión?" antes de mostrar cualquier página.

- **`Services/Auth/AuthState.cs`** — scoped (vive un circuito de Blazor
  Server = una pestaña del navegador). Guarda `IsAuthenticated`, `Token`,
  `NombreCompleto`, `RolNombre`, y el set de códigos de permiso del usuario
  logueado. **El JWT nunca sale al navegador** — Blazor Server ejecuta todo
  del lado del servidor, así que `AuthState` vive en memoria del proceso,
  nunca en `localStorage`/cookie/JS. `HasPermission(codigo)` es el único
  método que usan las páginas para gatear UI.
- **El Bearer se fija en el constructor de cada `*ApiClient` autenticado**
  (Producto/Movimiento/Solicitud/Categoria/Area/Ubicacion/Conteo/Usuario/Rol),
  no vía un `DelegatingHandler` de `IHttpClientFactory` — **se probó ese
  camino primero (`AuthHeaderHandler` + `.AddHttpMessageHandler<T>()`) y
  falla en silencio: todo el CRUD real daba 401 sin error visible.** Causa
  raíz, encontrada recién con la app corriendo de punta a punta (login
  funcionaba, pero `/inicio` nunca cargaba nada): `IHttpClientFactory`
  arma el pipeline de handlers en su **propio scope de DI interno**
  (`IServiceScopeFactory.CreateScope()`, cacheado ~2 min), no en el scope
  del circuito de Blazor — un `DelegatingHandler` que pide `AuthState` ahí
  nunca ve el del usuario real logueado, así que el header Bearer nunca se
  agregaba. El *typed client* (`ProductoApiClient`, etc.) en cambio SÍ se
  resuelve en el scope correcto (el del circuito) cuando un componente lo
  inyecta con `@inject` — por eso cada `*ApiClient` ahora recibe
  `HttpClient` **y** `AuthState` en su constructor y fija
  `_http.DefaultRequestHeaders.Authorization` ahí mismo, una sola vez
  (alcanza: el token no cambia durante la vida del circuito — logout hace
  `forceLoad`, circuito nuevo, `ApiClient` nuevo con el token correcto de
  la próxima sesión). **`AuthApiClient` (login) sigue sin tocar esto** —
  todavía no hay token cuando se loguea. Si se agrega un `*ApiClient`
  nuevo que necesite auth, copiar este patrón — no reintroducir un
  `DelegatingHandler` con estado scoped, es la misma trampa.
- **Login**: `Components/Pages/Home.razor`, ruta `/`, `WelcomeLayout`. Llama
  a `AuthApiClient.LoginAsync` → `AuthState.SignIn(resultado)` → navega a
  `/inicio`. Si ya hay sesión activa (`AuthState.IsAuthenticated`), redirige
  directo a `/inicio` en `OnInitialized` (no vuelve a mostrar el form).
- **Gate de sesión**: `MainLayout.OnInitialized` — si `!AuthState.IsAuthenticated`,
  `Nav.NavigateTo("/", forceLoad: true)`. Como `MainLayout` es el layout por
  defecto de **todas** las páginas salvo Login, un solo chequeo cubre toda
  la app. `forceLoad: true` fuerza una recarga completa del navegador (nuevo
  circuito) — necesario porque `AuthState` es scoped al circuito: sin
  `forceLoad`, redirigir dentro del mismo circuito no limpia nada raro, pero
  es el mismo patrón que usa "Cerrar sesión" y ahí sí hace falta un circuito
  nuevo para que el `AuthState` viejo (ya deslogueado) no persista.
- **Visibilidad del rail y de los botones de acción por permiso** — cada
  `RailLink` del sidebar y cada botón de crear/editar/aprobar/rechazar/
  entregar en las páginas de Productos, Movimientos, Solicitudes,
  Categorías, Áreas y Ubicaciones está envuelto en
  `@if (AuthState.HasPermission(Permisos.XxxYyy))`. `Permisos` es
  `Inventory.Domain.Security.Permisos` — **el mismo catálogo que usa el
  backend**, llega acá por la referencia a `Inventory.Application` (que a su
  vez referencia `Inventory.Domain`). Si el backend agrega un permiso nuevo,
  este proyecto lo ve al recompilar, sin duplicar el código del permiso acá.
- **`/usuarios` y `/roles`** (nuevas, `Components/Pages/Usuarios/Index.razor`
  y `Components/Pages/Roles/Index.razor`) — gestión de usuarios (alta con
  contraseña inicial, editar nombre/rol/activo) y roles (alta/edición con
  checkboxes de permisos agrupados por módulo, vía
  `RolApiClient.ListarPermisosDisponiblesAsync`). Solo aparecen en el rail
  si el usuario tiene `usuarios.gestionar`/`roles.gestionar` — y el backend
  igual los exige en el endpoint, así que ocultar el link no es la única
  defensa.
- **Sin implementar**: cambio/reseteo de contraseña, refresco de token (si
  expira a las 8h con la pestaña abierta, el próximo request cae en 401 y el
  usuario ve el mensaje de `ApiClientHelper` pidiendo volver a loguearse —
  no hay renovación automática).

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
- **Ubicaciones sin Editar/Eliminar** — el mockup tenía ese botón; el backend
  solo expone `Listar`+`Crear` para `Ubicacion` (ver `InventoryPlatform/CLAUDE.md`,
  sección "Operaciones que el backend no expone todavía"). No agregar el
  botón acá hasta que el backend tenga el endpoint.
  **Categorías y Áreas SÍ tienen Editar/Eliminar desde 2026-09-15** —
  `Categorias/Index.razor` y `Areas/Index.razor` reusan el mismo diálogo de
  crear con un botón "Editar" por fila; "eliminar" es destildar el checkbox
  "Activa" del formulario de edición y guardar (llama al mismo
  `ActualizarAsync` con `Activo: false`), no hay un endpoint de DELETE
  aparte. La categoría/área sigue en la base, solo deja de listarse (el
  `ListarAsync` que usan estas páginas no pide `incluirInactivas`) y de
  poder elegirse en productos/solicitudes nuevos.
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
| `/unidades` | Unidades de medida | Tabla + diálogo de alta/edición. El código es de solo lectura al editar (ver abajo) |

## Comandos

```bash
dotnet build
dotnet run --launch-profile https      # https://localhost:7126
```

**Necesita el backend corriendo en paralelo** (`InventoryPlatform/src/Inventory.Api`,
`https://localhost:7272`) — sin eso, todas las páginas muestran el error
genérico controlado ("Ocurrió un error inesperado") en vez de datos, que es
el comportamiento esperado y ya verificado, no un bug.

## Unidades de medida (agregado 2026-09-16)

`/unidades` (`Components/Pages/Unidades/Index.razor`) administra el catálogo de unidades
que antes eran 3 `<option>` fijas en el formulario de producto. Mismo patrón visual que
Categorías y Áreas: tabla + diálogo, "eliminar" es destildar «Activa».

- **El campo Código está deshabilitado al editar** — el backend no lo expone en
  `ActualizarUnidadDto` porque ese código va embebido en la `ClaveProducto` de cada
  producto que lo usa (ver `InventoryPlatform/CLAUDE.md`). No agregar un input editable
  acá: el PUT lo ignoraría igual.
- **`Productos/Index.razor` carga el `<select>` de unidad desde `UnidadApiClient`**, no
  de una lista fija, y el valor por defecto al crear es la primera unidad del catálogo,
  ya no `"UNI"` — que podría estar desactivada.
- El rail muestra el link con `unidades.ver`, que `Operador` y `Consulta` ya traen
  seedeado (lo necesitan para el formulario de producto).
- `Icon.razor` tiene el ícono `ruler` agregado para este módulo.

## Lo que NO hacer

- ❌ No dupliques DTOs a mano — vienen de `Inventory.Application` por
  referencia de proyecto.
- ❌ No agregues acceso a datos ni `Inventory.Infrastructure` acá — todo pasa
  por los `*ApiClient` de `Services/`.
- ❌ No agregues un botón de Editar/Eliminar a Ubicaciones hasta que el
  backend exponga ese endpoint (Categorías y Áreas ya lo tienen).
- ❌ No vuelvas a hardcodear las unidades de medida en un `<select>` — salen de
  `UnidadApiClient.ListarAsync()`. Y no hagas editable el código de una unidad ya
  creada (ver "Unidades de medida").
- ❌ No adjuntes el JWT vía `DelegatingHandler` + `.AddHttpMessageHandler<T>()`
  para un `HttpClient` tipado — `IHttpClientFactory` resuelve ese handler en
  un scope de DI propio, no en el del circuito, así que un `AuthState`
  scoped inyectado ahí siempre está deslogueado y el Bearer nunca se manda
  (401 en silencio en todo el CRUD, encontrado ya con la app corriendo — ver
  "Autenticación y permisos"). Fijar el header en el constructor del
  `*ApiClient` mismo, que sí se resuelve en el scope correcto.
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
