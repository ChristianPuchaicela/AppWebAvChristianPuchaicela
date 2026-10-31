# PoliMusic — Reproductor de música web (ASP.NET Web Forms, .NET Framework 4.8)

Aplicación web tipo Spotify construida con **ASP.NET Web Forms clásico**, C#, ADO.NET y
**SQL Server**, con arquitectura en tres capas y una identidad visual vino/rosa.

El repositorio contiene **dos proyectos**:

| Carpeta | Tipo de proyecto | Estado |
|---|---|---|
| `webapppolimusicv1_asp/` | *Web Site* (sin `.csproj`, con `App_Code`, `CodeFile=`) | Versión anterior, se conserva como referencia |
| `SlnAppPoliMusic/WebAppPoliMusicV2/` | **Web Application Project** (`BLL`/`DAL`/`Entities`, code-behind con `.designer.cs`) | **Versión actual — usar esta** |

---

## Índice

1. [Requisitos](#1-requisitos)
2. [Puesta en marcha](#2-puesta-en-marcha)
3. [Estructura del repositorio](#3-estructura-del-repositorio)
4. [Arquitectura de la solución V2](#4-arquitectura-de-la-solución-v2)
5. [Inventario de páginas](#5-inventario-de-páginas)
6. [Modelo de datos](#6-modelo-de-datos)
7. [Sistema de diseño](#7-sistema-de-diseño)
8. [Seguridad](#8-seguridad)
9. [Errores corregidos respecto a la versión anterior](#9-errores-corregidos-respecto-a-la-versión-anterior)
10. [Estado de verificación](#10-estado-de-verificación)
11. [Tareas pendientes conocidas](#11-tareas-pendientes-conocidas)

---

## 1. Requisitos

| Componente | Versión / nota |
|---|---|
| Windows | 10 / 11 / Server |
| Visual Studio | 2019 o 2022, con la carga de trabajo **ASP.NET y desarrollo web** |
| .NET Framework | 4.8 (Runtime + Developer Pack) |
| SQL Server | 2019 / 2022 / 2025 — instancia accesible como `localhost` |
| Navegador | Chrome / Edge / Firefox con soporte de `<audio>` |

No hay dependencias NuGet. Todo se resuelve con ensamblados del GAC
(`System`, `System.Core`, `System.Data`, `System.Web`, `System.Web.Extensions`,
`System.Configuration`, `System.Drawing`, `System.Xml`).

---

## 2. Puesta en marcha

### Paso 1 — Crear la base de datos

La base **`BDD_PoliMusic`** ya existe en el entorno de la cátedra y **no debe
modificarse su estructura**. El script original se conserva intacto en
`BDD_PoliMusic.sql` (codificación UTF-16LE) y crea las 9 tablas y los datos de
prueba.

Si necesita recrearla desde cero:

```bat
sqlcmd -S localhost -U SA -P SU_CLAVE -C -i BDD_PoliMusic.sql
```

**Opcional, no destructivo.** `db/01_PoliMusic_incremental.sql` corrige
incoherencias de *datos* (no de esquema) que rompían la aplicación:
normaliza el uso de mayúsculas en `TBL_PLAYLIST.PHOTO` y deja una cuenta con
contraseña ya cifrada. Es **idempotente**, se puede ejecutar varias veces:

```bat
sqlcmd -S localhost -U SA -P SU_CLAVE -C -i db\01_PoliMusic_incremental.sql
```

### Paso 2 — Revisar la cadena de conexión

A diferencia de la v1 —que la escondía en `Web.secrets.config`—, en la V2 la
cadena vive directamente en **`SlnAppPoliMusic/WebAppPoliMusicV2/Web.config`**
con el mismo nombre que ya usaba el proyecto original:

```xml
<connectionStrings>
  <add name="BDD_PoliMusicConnectionString"
       connectionString="Data Source=localhost;Initial Catalog=BDD_PoliMusic;User ID=usr_polimusic;Password=Politecnica1"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Si su instancia usa otro servidor, puerto o credenciales, **cambie solo el
atributo `connectionString`**. Ninguna otra parte del código conoce la
contraseña: todo pasa por `DAL/Db.cs`.

> Si prefiere no versionar el secreto, mueva ese bloque a un archivo aparte y
> use `<connectionStrings configSource="Web.secrets.config" />`. El código no
> cambia porque siempre lee por nombre (`Db.ConnectionString`).

### Paso 3 — Abrir y ejecutar

```
SlnAppPoliMusic\SlnAppPoliMusic.slnx
```

Establezca `WebAppPoliMusicV2` como proyecto de inicio y ejecute con **IIS
Express** (F5). La página de arranque es `Default.aspx`.

También puede publicar en IIS local: la aplicación usa `System.WebServer`, así
que funciona igual en el módulo integrado.

### Cuentas de prueba (vienen en `BDD_PoliMusic.sql`)

| Usuario | Contraseña | Rol |
|---|---|---|
| `userAdmin` | `12345678` | Administrador |
| `userNormal` | `12345678` | Usuario normal |
| `username1` | *(SHA-1 heredado)* | Usuario normal |

La contraseña `12345678` está almacenada en **texto plano** en el script
original. La aplicación la acepta y, tras el primer inicio de sesión correcto,
la **re-cifra automáticamente** con PBKDF2 sin que el usuario note nada.

### Carpetas de recursos

La base guarda rutas relativas a la raíz (`~/songFiles/...`,
`~/ArtistImages/...`). Los archivos deben existir en el proyecto V2:

```
WebAppPoliMusicV2/
├── AlbumImages/     ← album1..5.png
├── ArtistImages/    ← artist1..4.jpg + artist5.png
├── SongFiles/       ← bensound-adventure|dreams|energy|scifi|tomorrow.mp3
├── UserImages/      ← default.jpg, root.png, userNormal.png, username1.png
└── PlaylistImages/  ← default.jpg + playlist1..5.png
```

**No hace falta tocar la base de datos para corregir mayúsculas.**
El script original guarda rutas con capitalización inconsistente
(`~/userImages/default.jpg`, `~/albumImages/album1.png`,
`~/ArtistImages/artist1.jpg`, `~/playlistImages/Playlist2.png`) y con
extensiones mezcladas `.jpg` / `.png`. `Util/MediaResolver.cs` resuelve la ruta
guardada contra el sistema de archivos real en tres pasos —ruta exacta →
búsqueda sin distinguir mayúsculas carpeta por carpeta → carpeta por defecto— y
cachea el resultado en memoria. Así `~/songFiles/bensound-adventure.mp3`
(minúscula en la BD) encuentra `SongFiles/bensound-adventure.mp3` (mayúscula en
disco) sin necesidad de un `UPDATE`.

---

## 3. Estructura del repositorio

```
AppWebAvChristianPuchaicela/
├── BDD_PoliMusic.sql                 Script original de la base (UTF-16LE)
├── db/
│   └── 01_PoliMusic_incremental.sql  Correcciones de datos, idempotente
├── PoliMusic.sln                     Solución de la v1 (Web Site)
├── SongFilesAndImages_resources/     Medios originales sin organizar
├── webapppolimusicv1_asp/            v1 — Web Site con App_Code (referencia)
└── SlnAppPoliMusic/
    ├── SlnAppPoliMusic.slnx
    └── WebAppPoliMusicV2/            v2 — Web Application Project (ACTUAL)
```

---

## 4. Arquitectura de la solución V2

Capas estrictas, sin saltos: **la página nunca escribe SQL y el DAL nunca
decide reglas de negocio**.

```
┌──────────────────────────────────────────────────────────┐
│  Presentación   *.aspx / *.aspx.cs / Site.master          │
│                 Common/PageBase.cs   Common/Ui.cs          │
├──────────────────────────────────────────────────────────┤
│  Negocio        BLL/*.cs                                  │
│                 AuthService, UserBll, SongBll, ...        │
│                 Devuelven OperationResult (mensaje+ok)     │
├──────────────────────────────────────────────────────────┤
│  Datos          DAL/*.cs   (ADO.NET, todo parametrizado)  │
│                 DAL/Db.cs  ← único punto de conexión       │
├──────────────────────────────────────────────────────────┤
│  Modelo         Entities/*.cs  (POCOs sin lógica de BD)    │
└──────────────────────────────────────────────────────────┘
         Util/  → PasswordHasher, UserSession, MediaResolver,
                  FileStorage, Constants
```

### `Entities/` — 6 POCOs

`User`, `Song`, `Artist`, `Album`, `Playlist`, `CatalogItem`.
Solo propiedades y ayudantes de presentación (`IsAdmin`, `RoleName`,
`Initials`, `Age`, `FullName`, `ReleaseLabel`, `IsPrivate`, `VisibilityName`,
`PlayCount`). Ninguna referencia a `System.Data`.

### `DAL/` — 7 archivos

| Archivo | Responsabilidad |
|---|---|
| `Db.cs` | `ConnectionString`, `Query<T>`, `QuerySingle<T>`, `ExecuteNonQuery`, `ExecuteScalarInt`, `InsertReturningId`, `ExecuteTransaction(params)`, helpers `P(name, value)` y `ToInt/ToStr/ToDate` |
| `UserDal.cs` | Login, alta, cambio de contraseña, listado y borrado |
| `SongDal.cs` | CRUD + `ARTIST_LABEL` con `STUFF(... FOR XML PATH(''))` y `ALBUM_TITLE` por subconsulta |
| `ArtistDal.cs` | CRUD, borrado en cascada transaccional |
| `AlbumDal.cs` | CRUD, tracklist (`TBL_TRACKLIST`), cascada transaccional |
| `PlaylistDal.cs` | CRUD, canciones de la lista (`TBL_PLAYLIST_SONG`), visibilidad |
| `CatalogDal.cs` | Lectura de `TBL_CATALOG` (tipos de usuario y de playlist) |

Todas las consultas usan `SqlParameter`. **No existe una sola concatenación de
valores en SQL** en todo el proyecto.

### `BLL/` — 8 archivos

`OperationResult` (`.Ok`, `.Message`, `.Errors`), `AuthService`
(`AuthResult`, `Authenticate`, `Register`, `ChangePassword`), `UserBll`,
`SongBll`, `ArtistBll`, `AlbumBll`, `PlaylistBll`, `CatalogBll`.

Aquí viven las reglas: validación de campos obligatorios, longitud máxima
según el esquema (`VARCHAR(50)` para nombres, títulos, correo y contraseña),
unicidad de usuario/correo, contraseña mínima de 8 caracteres, y la
**propiedad de las playlists** (`ReadOwned` / `ReadReadable` / `DeleteOwned`).

### `Util/` — 5 archivos

| Archivo | Detalle |
|---|---|
| `Constants.cs` | `ID_USER_NORMAL=0`, `ID_USER_ADMIN=1`, `ID_PLAYLIST_PUBLIC=0`, `ID_PLAYLIST_PRIVATE=1`, claves de sesión `PM_USER`/`PM_CSRF`/`PM_FLASH`, límites de longitud |
| `PasswordHasher.cs` | PBKDF2-SHA256, 10 000 iteraciones, formato `PBH1$<saltHex>$<hashHex>` = 46 caracteres (cabe en `VARCHAR(50)`). Verifica además SHA-1 y texto plano heredados; `NeedsUpgrade()` indica cuándo migrar |
| `UserSession.cs` | `SignIn`, `SignOut`, `Current`, `IsAdmin`, `SetFlash`/`ConsumeFlash` |
| `MediaResolver.cs` | Resolución de rutas con caché, insensible a mayúsculas |
| `FileStorage.cs` | `SaveImage` / `SaveSong` para `FileUpload` y `HttpPostedFile`, límites 5 MB (imagen) y 50 MB (mp3), `SanitizeFileName`, `DeleteIfExists` |

### `Common/` — 2 archivos

**`PageBase.cs`** — la clase de la que heredan todas las páginas:

* `IsPublicPage` (por defecto `false` → **acceso denegado por defecto**)
* `RequiresAdmin`
* `EnforceAuthorization()` en `OnInit`, antes de procesar controles
* `CsrfToken` / `ValidateCsrf()` con `__PoliMusicToken`
* Lectura segura de entrada: `FormInt`, `FormStr`, `FormBool`, `FormDate`,
  `QueryInt`, `QueryStr`
* `RedirectWithFlash(url, mensaje, esError)` para POST-Redirect-GET
* `H()` / `A()` / `Url()` para escapar HTML

**`Ui.cs`** — generadores de HTML en C#. Se usa esta vía en lugar de `GridView`
para tener control total del diseño y para no anidar formularios dentro del
único `<form runat="server">` del master:

`E`, `A`, `InitialsOf`, `Art`, `PlayPayload`, `PlayQueue`, `SongCard`,
`AlbumCard`, `ArtistCard`, `PlaylistCard`, `SongRow`, `EmptyState`, `Alert`,
`StatCard`, `SectionHead`, `PageHead`, `CsrfField`, `Submit`.

### Raíz del proyecto

`Site.master` (+ `.cs` / `.designer.cs`) contiene la barra lateral con SVG
en línea, la barra superior con buscador y chip de usuario, el **reproductor
persistente** y el contenedor de avisos. El `<form runat="server">` lleva
`enctype="multipart/form-data"` para que las subidas funcionen desde cualquier
página sin formularios anidados.

`Content/Site.css` (~1000 líneas, 12 secciones) y `Scripts/pollmusic.js`
(reproductor persistente con `sessionStorage`, cola de reproducción, contador
por AJAX, filtros `[data-pm-filter]`, modales, confirmaciones).

---

## 5. Inventario de páginas

19 páginas `.aspx`, cada una con su `.aspx.cs` y `.aspx.designer.cs`.

### Públicas

| Página | Acceso | Función |
|---|---|---|
| `Login.aspx` | Público | Inicio de sesión; respeta `?returnUrl=` |
| `SignUp.aspx` | Público | Registro con foto opcional |
| `Logout.aspx` | Público | Cierra sesión y limpia el carrito de reproducción |
| `AccesoDenegado.aspx` | Público | 403 amable con enlace de vuelta |
| `Error.aspx` | Público | Error genérico (clase `ErrorPage`) |
| `Play.aspx` | Público | Endpoint AJAX del contador de reproducciones; responde `204/401/403/405` |

### Requieren sesión

| Página | `ActiveSection` | Función |
|---|---|---|
| `Default.aspx` | `home` | Panel con estadísticas, top de canciones, álbumes, artistas y playlists |
| `Canciones.aspx` | `songs` | Catálogo de canciones con buscador y cola de reproducción |
| `Artistas.aspx` | `artists` | Listado; con `?id=` muestra el detalle del artista |
| `Albumes.aspx` | `albums` | Listado; con `?id=` muestra el tracklist |
| `Playlists.aspx` | `playlists` | Playlists propias y públicas; alta por modal |
| `PlaylistDetalle.aspx` | `playlists` | Añadir/quitar canciones, editar y eliminar (solo el dueño) |
| `Perfil.aspx` | `profile` | Datos personales, foto y cambio de contraseña |

### Administración (`RequiresAdmin = true`, carpeta `Admin/`)

| Página | `ActiveSection` | Función |
|---|---|---|
| `Admin/Default.aspx` | `admin` | Resumen del panel |
| `Admin/Canciones.aspx` | `admin-songs` | CRUD de canciones + subida de mp3 |
| `Admin/Artistas.aspx` | `admin-artists` | CRUD de artistas + foto |
| `Admin/Albumes.aspx` | `admin-albums` | CRUD de álbumes, portada y tracklist |
| `Admin/Playlists.aspx` | `admin-playlists` | CRUD de playlists de cualquier usuario |
| `Admin/Usuarios.aspx` | `admin-users` | Listado, cambio de rol y baja de usuarios |

Los formularios con varias acciones usan **un solo submit** y se distinguen por
el valor de `name="action"`; nunca hay `<form>` anidados.

### ¿Dónde está `Song.aspx`?

**No existe en la V2.** La v1 tenía una página `Song.aspx` que enlazaba la
columna `PLAY_COUNT`, inexistente en `BDD_PoliMusic` (la real es `PLAYS`), así
que se eliminó. Su funcionalidad está repartida en tres páginas y tres capas:

| Qué | Dónde |
|---|---|
| Catálogo y búsqueda de canciones | `Canciones.aspx` + `Canciones.aspx.cs` |
| Contador de reproducciones (AJAX) | `Play.aspx` + `Play.aspx.cs` |
| Alta, edición y borrado | `Admin/Canciones.aspx` + `Admin/Canciones.aspx.cs` |
| Modelo | `Entities/Song.cs` |
| Acceso a datos | `DAL/SongDal.cs` |
| Reglas de negocio | `BLL/SongBll.cs` |

La reproducción en sí **no tiene página propia**: el reproductor vive en
`Site.master` y `Scripts/pollmusic.js`, y las canciones se le entregan
serializadas en `data-pm-play` por `Ui.PlayPayload`.

---

## 6. Modelo de datos

Base **`BDD_PoliMusic`** — 9 tablas. **El esquema no se ha modificado.**

| Tabla | Columnas principales |
|---|---|
| `TBL_CATALOG` | `ID_CATALOG`, `COD_CATALOG_PARENT`, `COD_CATALOG`, `VALUE` |
| `TBL_SONG` | `ID_SONG`, `SONG_NAME` `varchar(50)`, `SONG_PATH` `varchar(255)`, `PLAYS` |
| `TBL_USER` | `ID_USER`, `USERNAME`, `PASSWORD` `varchar(50)`, `EMAIL`, `BIRTHDAY`, `USER_PHOTO`, `USER_TYPE` |
| `TBL_ALBUM` | `ID_ALBUM`, `TITLE_ALBUM`, `RELEASE_YEAR`, `ALBUM_IMAGE_PATH` |
| `TBL_TRACKLIST` | `ID_ALBUM` + `ID_SONG` (PK compuesta) |
| `TBL_ARTIST` | `ID_ARTIST`, `ARTIST_NAME`, `ARTIST_LASTNAME`, `ARTIST_IMAGE` |
| `TBL_PLAYER` | `ID_SONG` + `ID_ARTIST` (PK compuesta) |
| `TBL_PLAYLIST` | `ID_PLAYLIST`, `ID_USER`, `TITLE`, `CREATION_DATE`, `TYPE`, `PHOTO` |
| `TBL_PLAYLIST_SONG` | `ID_PLAYLIST` + `ID_SONG` (PK compuesta) |

**Catálogos** (`TBL_CATALOG`): padre `1` = tipo de usuario (`0` normal,
`1` admin); padre `2` = tipo de playlist (`0` pública, `1` privada). Esos
valores están replicados en `Util/Constants.cs`.

**Relaciones sin `FOREIGN KEY`.** El esquema original no declara claves
foráneas, así que la integridad se garantiza en `DAL` con transacciones
explícitas (`ExecuteTransaction`): al borrar un artista, un álbum o una
playlist se eliminan primero sus filas en `TBL_PLAYER`, `TBL_TRACKLIST` o
`TBL_PLAYLIST_SONG`.

**`PLAYS` es la columna del contador**, no `REPRODUCCIONES`.

---

## 7. Sistema de diseño

Identidad vino/concha de vino, definida con variables CSS en `Content/Site.css`:

```css
--wine-950  --wine-900  --wine-800  --wine-700  --wine-600
--rose-500  --rose-400  --rose-300  --rose-200
```

* Barra lateral fija con iconos SVG en línea y estado activo por `ActiveSection`.
* Barra superior tipo *glass* con buscador y chip de usuario.
* Tarjetas con portada, degradado y superposición de acciones al pasar el ratón.
* Tablas de canciones con numeración, duración y acciones por fila.
* Pantallas de autenticación partidas en dos mitades (arte + formulario).
* Reproductor persistente anclado al pie: sobrevive a la navegación entre
  páginas mediante `sessionStorage`, permite avanzar/retroceder en la cola y
  envía el contador por `fetch` a `Play.aspx`.
* Estados vacíos, avisos (*flash*) que se ocultan solos, modales y
  confirmaciones nativos estilizados.
* Puntos de ruptura responsivos para escritorio, tableta y móvil.

---

## 8. Seguridad

| Riesgo | Mitigación |
|---|---|
| Acceso sin sesión | `PageBase` deniega por defecto; la comprobación ocurre en `OnInit`, en servidor |
| Escalada a administrador | `RequiresAdmin` + `Admin/*` protegido; `AccesoDenegado.aspx` |
| Inyección SQL | 100 % consultas parametrizadas en `DAL` |
| XSS | Todo texto dinámico pasa por `Ui.E()` / `H()` / `A()` |
| CSRF | Token por sesión en `__PoliMusicToken`, validado en cada POST |
| Contraseñas | PBKDF2-SHA256 con salt; comparación en tiempo constante; migración transparente desde SHA-1 y texto plano |
| Fijación de sesión | `UserSession.SignIn` renueva la sesión y el token CSRF |
| Subida de archivos | Extensión y tamaño validados (5 MB imagen / 50 MB mp3), nombre saneado, carpeta destino fija |
| Fuga de código fuente | `requestFiltering` bloquea `.cs`, `.csproj`, `.sln` y `Web.config` |
| Clickjacking / MIME sniffing | `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy` |
| Cookies | `httpOnlyCookies="true"`, `sameSite="Lax"` |

La autenticación es **propia** (`<authentication mode="None" />`), no
`FormsAuthentication`. Se eligió así para no mezclar dos mecanismos y para
poder apoyar la autorización en una sola clase (`PageBase`).

---

## 9. Errores corregidos respecto a la versión anterior

Estos son los defectos reales que traía el proyecto V2 heredado y que
impedían que compilara o funcionara:

| # | Archivo | Error | Corrección |
|---|---|---|---|
| 1 | `WebAppPoliMusicV2.csproj` | `<TargetFrameworkVersion>v2.0</TargetFrameworkVersion>` | `v4.8` — imprescindible para `Rfc2898DeriveBytes` con `HashAlgorithmName` |
| 2 | `DAL/Song.cs` | Consultaba la columna `REPRODUCCIONES`, que no existe | Reescrito contra `PLAYS`; se añadió `ARTIST_LABEL` y `ALBUM_TITLE` |
| 3 | `BLL/Song.cs` | Propagaba la columna inexistente | Reescrito con validaciones y `OperationResult` |
| 4 | `Song.aspx` | Enlazaba `PLAY_COUNT` (campo inexistente) | Página eliminada; el catálogo es `Canciones.aspx` |
| 5 | `Login.aspx.cs` | `Response.Redirect("~/DesktopAdmin.aspx")` — esa página nunca existió | Los administradores van a `~/Default.aspx`; existe un panel real en `Admin/Default.aspx` |
| 6 | `Util/Hash.cs` | Solo SHA-1, incompatible con las cuentas PBKDF2 | Sustituido por `Util/PasswordHasher.cs` (PBKDF2 + SHA-1 + texto plano) |
| 7 | `FileManagement.cs` | Heredaba de `System.Web.UI.Page` sin ser una página | Sustituido por la clase estática `Util/FileStorage.cs` |
| 8 | `Playlists.aspx`, `PlaylistDetalle.aspx` | `<form>` anidados dentro del `<form runat="server">` del master | Un único formulario; acciones distinguidas por `name="action"` |
| 9 | Paneles de búsqueda | `<form action="...">` habría eliminado el formulario del master | `.pm-filter` + `data-pm-filter` con `applyFilter()` en JS |
| 10 | `CatalogBll.cs` | Estaba en una carpeta equivocada (`AppWebPoliMusic/`) | Movido a `BLL/`; carpeta sobrante eliminada |
| 11 | `SignUp.aspx` | Usaba `FormValues["username"]`, que no existía | Propiedades `UserNameValue` / `EmailValue` / `BirthdayValue` en el code-behind |
| 12 | `PageBase.cs` | Variables `out` declaradas en línea (C# 7) | Reescrito con variables predeclaradas |
| 13 | `Site.master.cs` | `!IsPostBack` no existe en `MasterPage` | `!Page.IsPostBack` |
| 14 | `Albumes.aspx` | `"\u00C1lbumes"` como escape C# dentro del marcado | Literal `"Álbumes"` |
| 15 | Varias páginas | Entidades HTML (`&iacute;`, `&mdash;`) dentro de argumentos de C# | Texto acentuado directo y `&mdash;` emitido de forma condicional |
| 16 | `SlnAppPoliMusic.slnx` | Nombre de carpeta obsoleto `..._ASPX_F2.0` | `PoliMusic`; se eliminó un `.slnx` vacío en la raíz |
| 17 | `Util/UserSession.cs` | `HttpSessionState` sin `using System.Web.SessionState` → *"no se encontró el nombre del tipo"* | Directiva añadida |
| 18 | `Web.config` | `<fileExtensions>` redeclaraba `.cs`, `.csproj`, `.sln`, ya denegadas por IIS → **HTTP 500.19 / 0x800700b7** | Bloque eliminado; IIS ya las bloquea y trata `web.config` como *hiddenSegment* |
| 19 | `.vs/.../config/applicationhost.config` | El sitio estaba en `Clr2IntegratedAppPool` → *"Atributo 'targetFramework' no reconocido"* con **ASP.NET 2.0.50727** | Cambiado a `Clr4IntegratedAppPool` |
| 20 | `Util/MediaResolver.cs` | Devolvía `~/UserImages/default.jpg` y se escribía tal cual en `<img src>` y en el JSON del reproductor → **ninguna imagen ni audio cargaba** | Nuevo `ToUrl()` con `VirtualPathUtility.ToAbsolute`: devuelve `/UserImages/default.jpg` |

### Sobre las imágenes y el audio que no cargaban

`MediaResolver` encontraba el archivo correctamente, pero devolvía la ruta
**lógica** de ASP.NET:

```html
<img src="~/UserImages/default.jpg">
```

El navegador no entiende `~/`. Ese prefijo solo lo resuelven ASP.NET
(`ResolveUrl`, controles de servidor) y `Server.MapPath`. Al recibirlo literal,
el navegador pide `http://localhost:56575/~/UserImages/default.jpg` y obtiene
404; el `onerror` de `Ui.Art` ocultaba la imagen y quedaba solo el degradado
vino del contenedor. Lo mismo ocurría con el `src` de las pistas en el JSON del
reproductor, así que **tampoco sonaba la música**.

Por eso los estilos y los enlaces sí funcionaban: `Site.master` los emitía con
`ResolveUrl(...)`, pero las imágenes pasaban por `MediaResolver.Resolve` y el
resultado se escribía directo.

La corrección se hizo en un solo punto, `MediaResolver.Resolve`, que es de donde
salen todas las rutas de medios:

```csharp
public static string ToUrl(string virtualPath)
{
    HttpContext ctx = HttpContext.Current;
    if (ctx == null || ctx.Request == null) return virtualPath;  // tiempo de diseno
    try { return VirtualPathUtility.ToAbsolute(virtualPath); }
    catch (Exception) { return virtualPath; }
}
```

`VirtualPathUtility.ToAbsolute` respeta el directorio virtual: con la aplicación
en la raíz devuelve `/UserImages/default.jpg` y publicada en `/PoliMusic`
devuelve `/PoliMusic/UserImages/default.jpg`. `FileStorage.DeleteIfExists` sigue
funcionando porque `Server.MapPath` acepta ambas formas.

### Sobre el error de `targetFramework` y ASP.NET 2.0

Este error no está en el código ni en `Web.config`: está en el **grupo de
aplicaciones** con el que IIS Express sirve el sitio.

```
Atributo 'targetFramework' no reconocido.
Versión de Microsoft .NET Framework: 2.0.50727.9188
Versión ASP.NET: 2.0.50727.9183
```

`targetFramework` en `<compilation>` y `<httpRuntime>` se añadió en **.NET 4.0**;
si el sitio corre sobre el CLR 2.0, el analizador de configuración no lo
reconoce. El proyecto venía con `TargetFrameworkVersion>v2.0`, y Visual Studio
creó el sitio en `.vs/<solución>/config/applicationhost.config` con el grupo
`Clr2IntegratedAppPool`. **Cambiar el `.csproj` a v4.8 no actualiza ese archivo**,
porque VS reutiliza la entrada del sitio que ya existe.

La corrección es una línea:

```xml
<site name="WebAppPoliMusicV2" id="2">
  <application path="/" applicationPool="Clr4IntegratedAppPool">
```

Si en el futuro el sitio vuelve a arrancar en v2.0, la vía limpia es cerrar
Visual Studio y borrar la carpeta `.vs/` de la solución: al reabrir, VS
regenera el `applicationhost.config` a partir del `TargetFrameworkVersion`
actual (v4.8) y asigna `Clr4IntegratedAppPool` por sí solo.

> **Requisito:** tener instalado el .NET Framework 4.8 (Release ≥ 528040).
> Verifíquelo en `HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full`.

### Sobre el 500.19

El error de configuración no venía de un archivo mal formado, sino de una
**entrada duplicada**. IIS resuelve `requestFiltering` fusionando su
`applicationHost.config` con el `Web.config` de la aplicación; si este último
vuelve a declarar una clave que ya existe en el nivel superior, la fusión falla:

```
HTTP Error 500.19 - Internal Server Error
Error Code: 0x800700b7
No se puede agregar una entrada de colección duplicada de tipo 'add'
con un atributo de clave única 'fileExtension' establecido en '.csproj'
```

La solución **no** es poner `<clear />` —eso desactivaría también las
protecciones por defecto, dejando `.config`, `.master` y `.resx` accesibles—,
sino **no redeclarar** lo que IIS ya bloquea. La lista por defecto incluye
`.asax`, `.ascx`, `.master`, `.skin`, `.browser`, `.sitemap`, `.config`, `.cs`,
`.csproj`, `.vb`, `.vbproj`, `.vsdisco`, `.webinfo`, `.licx`, `.resx` y
`.resources`. El `Web.config` de la V2 solo declara ahora `requestLimits`.

Además, la v1 usaba `App_Code` con `CodeFile=`, lo que impide compilar con
antelación y detectar errores hasta la primera petición. La V2 es un **Web
Application Project** con `CodeBehind=` y `.designer.cs`, de modo que el
compilador revisa todo el código al construir.

---

## 10. Estado de verificación

**La primera compilación y ejecución real las hizo el usuario, y encontró cuatro
errores** (recogidos como #17 a #20 en la tabla anterior): un `using` faltante en
`Util/UserSession.cs`, el 500.19 de `Web.config`, el sitio sirviéndose sobre el
CLR 2.0 y las rutas `~/` sin resolver que rompían todas las imágenes y el audio.
Los cuatro están corregidos.

Antes de eso, el entorno donde se generó el código bloqueaba `MSBuild.exe`,
`csc.exe` y el compilador Roslyn (*"Known Windows LOLBin"*), así que la
verificación inicial fue estática. Comprobaciones ejecutadas sobre los 69
archivos `.cs` y 51 archivos de contenido:

| Comprobación | Resultado |
|---|---|
| Directivas `using` vs tipos usados en cada archivo | 1 faltante → corregido (#17) |
| Balance de llaves y paréntesis en todos los `.cs` | Sin desajustes |
| Formularios anidados por página y master | Exactamente 1 `<form>` en cada archivo |
| `Inherits` / `CodeBehind` vs clase y namespace reales | 19 páginas + master correctas |
| Controles usados en code-behind vs `.designer.cs` | Sin controles sin declarar |
| Aridad de llamadas a clases propias vs firmas | Sin desajustes |
| Llamadas `Ui.*` en el marcado vs firmas | Sin desajustes |
| Coherencia `.csproj` ↔ disco | 69/69 archivos `.cs` y 51/51 de contenido incluidos; ninguna entrada huérfana |
| Columnas SQL contra `BDD_PoliMusic.sql` | Todas las columnas usadas existen con el nombre y tipo correctos |

Si aparece un nuevo error de compilación, la causa más probable es otro `using`
faltante: el chequeo automático que se usó aquí solo cubría una lista de tipos
conocidos (`HttpSessionState`, `SqlConnection`, `ConfigurationManager`,
`Rfc2898DeriveBytes`, `FileUpload`, `JavaScriptSerializer`, etc.).

---

## 11. Tareas pendientes conocidas

* **Compilar y ejecutar** en un equipo con Visual Studio para confirmar el
  apartado anterior.
* La v1 (`webapppolimusicv1_asp/`) sigue en el repositorio como referencia.
  Cuando la V2 esté validada, puede eliminarse junto con `PoliMusic.sln`.
* `TBL_PLAYER` (relación artista–canción) existe en el esquema y el DAL la lee,
  pero todavía no hay pantalla para asignar varios artistas a una canción; hoy
  el artista se resuelve por la etiqueta agregada de `SongDal`.
* `SongFiles/` tiene 5 pistas, pero `BDD_PoliMusic.sql` solo inserta 4 filas en
  `TBL_SONG` (`bensound-tomorrow.mp3` queda sin registrar). Es un sobrante de
  los datos de prueba, no un error de la aplicación.

---

## Créditos

Trabajo académico de **Aplicaciones Web Avanzadas**.
Pistas de audio de demostración: [Bensound](https://www.bensound.com/).
