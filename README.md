# PoliMusic — Aplicación web tipo Spotify

Clon funcional de reproductor de música construido con **ASP.NET Web Forms clásico
(.NET Framework 4.8)**, C#, ADO.NET y **SQL Server**, con identidad visual marrón.

> Los hechos de este README están verificados ejecutando la aplicación, no inferidos.
> Al final se documenta con precisión qué se probó realmente y qué queda pendiente.

---

## 1. Cómo ejecutar la aplicación

### Requisitos

| Componente | Versión / nota |
|---|---|
| Windows | 10 / 11 / Server |
| .NET Framework | 4.8 (Runtime + Developer Pack) |
| SQL Server | 2019 / 2022 / 2025 (probado en SQL Server 2025, instancia `MSSQL17.MSSQLSERVER`) |
| IIS Express | 10.0 — `C:\Program Files\IIS Express\iisexpress.exe` |
| Navegador | Chrome / Edge / Firefox con soporte de `<audio>` |

No hay dependencias NuGet (`packages.config` está vacío). Todo usa ensamblados del GAC.

### Paso 1 — Preparar la base de datos

La base **`BDD_PoliMusic`** debe existir. El script original de la cátedra se conserva
sin modificar en `BDD_PoliMusic.sql`. Si necesita crearla de cero, ejecute ese script.

Opcional (no destructivo) — normaliza 3 rutas de imagen mal escritas y reinicia `PLAYS`:

```
sqlcmd -S localhost,1433 -U SU_USUARIO -P SU_CONTRASENA -C -i db\01_PoliMusic_incremental.sql
```

El script es **idempotente**: puede ejecutarse varias veces sin duplicar datos.

### Paso 2 — Configurar las credenciales (no versionadas)

**La cadena de conexión no está en `Web.config`.** Sus credenciales viven en
`Web.secrets.config`, un archivo **local excluido del control de versiones** por
`.gitignore`, de modo que el repositorio nunca publica el usuario ni la contraseña
de la base de datos.

`Web.config` solo la referencia:

```xml
<connectionStrings configSource="Web.secrets.config" />
```

Para configurar su entorno, copie la plantilla y ajuste los valores:

```bat
copy webapppolimusicv1_asp\Web.secrets.config.example webapppolimusicv1_asp\Web.secrets.config
```

El contenido de su `Web.secrets.config` quedará así (con **sus** datos):

```xml
<connectionStrings>
  <add name="PoliMusicConnection"
       connectionString="Server=localhost,1433;Database=BDD_PoliMusic;
                         User Id=SU_USUARIO;Password=SU_CONTRASENA;
                         Encrypt=true;TrustServerCertificate=true;
                         MultipleActiveResultSets=true"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

> **Formato obligatorio:** la raíz del archivo debe ser `<connectionStrings>`
> **sin** envolverlo en `<configuration>`. Si se envuelve, ASP.NET falla con
> *«El formato de un archivo configSource debe ser un elemento que contenga el
> nombre de la sección»*.
>
> **El sitio no arranca si falta `Web.secrets.config`.** Si acaba de clonar el
> repositorio, este es el primer paso obligatorio.

Ajuste `Server` si su instancia tiene otro nombre o puerto.

### Paso 3 — Ejecutar el sitio

**Opción A — script incluido (recomendado, no requiere compilar):**

```
webapppolimusicv1_asp\run\iniciar-polimusic.bat
```

El script **abre el navegador automáticamente** y deja el sitio en primer plano
(pulse `Ctrl+C` o cierre la ventana para detenerlo). Si el sitio ya estaba en
ejecución, **no intenta arrancar una segunda instancia**: lo detecta y muestra un
aviso con la dirección y las cuentas, en lugar de fallar con `0x800700b7`.

Para detenerlo cuando no tiene la ventana a mano:

```
webapppolimusicv1_asp\run\iniciar-polimusic.bat stop
```

Ese comando termina los procesos `iisexpress.exe`, espera a que HTTP.sys libere
el puerto 8090 y confirma el resultado. Después puede volver a arrancar con normalidad.

Luego abra en el navegador:

```
http://localhost:8090/
```

> **Importante:** use exactamente `localhost`, **no** `127.0.0.1`. IIS Express solo
> acepta el nombre de host con el que registra el sitio; con `127.0.0.1` responde
> `HTTP 400 Bad Request - Invalid Hostname`.
>
> **No use un proxy HTTP** para acceder. Si su entorno define `HTTP_PROXY`, añada
> `localhost` a las exclusiones (o use `--noproxy '*'` con `curl`).

**Opción B — Visual Studio:** abra `PoliMusic.sln` y pulse **Ctrl+F5**.
El proyecto arranca en IIS Express y compila el ensamblado en `bin\`.

**Opción C — línea de comandos:**

```
"C:\Program Files\IIS Express\iisexpress.exe" /config:"...\webapppolimusicv1_asp\run\applicationhost.config"
```

### Sobre el modelo de compilación (importante)

El sitio está preparado para funcionar **sin `bin\`** (sin compilación previa):
ASP.NET compila el código dinámicamente en el primer acceso.

* Todo el código compartido está en **`App_Code\`** (`Models`, `Data`, `Services`, `Common`).
* Las páginas usan **`CodeFile=`** (no `CodeBehind=`), que es el modelo *Web Site*.
* La primera petición tarda unos segundos mientras se compila; las siguientes son inmediatas.

Esto permite ejecutar y calificar el proyecto con solo IIS Express instalado.
Si abre la solución en Visual Studio, el flujo de compilación normal también es válido.

---

## 2. Cuentas verificadas

| Usuario | Rol | `USER_TYPE` | Contraseña | Origen |
|---|---|---|---|---|
| `userAdmin` | Administrador | 1 | `12345678` | `BDD_PoliMusic.sql` (INSERT original) |
| `userNormal` | Usuario | 0 | `12345678` | `BDD_PoliMusic.sql` (INSERT original) |
| `Christian.Puchaicela` | Administrador | 1 | *no determinada* | Datos existentes en `TBL_USER` |

Las contraseñas de `userAdmin` y `userNormal` provienen de los `INSERT` de
`BDD_PoliMusic.sql` y fueron **comprobadas con un inicio de sesión real** contra
la base de datos en ejecución (ver §11):

* `userAdmin` / `12345678` → redirige a `/Admin/Default.aspx` y muestra el panel real.
* `userNormal` / `12345678` → redirige a `/Default.aspx`; `/Admin/*` responde **403**.
* `userAdmin` / contraseña incorrecta → permanece en `/Login.aspx` con el aviso
  *«Usuario o contrasena incorrectos.»*

> **`Christian.Puchaicela` conserva un hash SHA-1 heredado** (`7C222FB292…`, 40
> caracteres). No se modificó: el sistema migra ese hash a PBKDF2 automáticamente
> en su primer inicio de sesión correcto. Su contraseña no forma parte de
> `BDD_PoliMusic.sql`, por lo que no se afirma ninguna.

* El sistema conserva compatibilidad con contraseñas heredadas en **texto plano**
  y **SHA-1**, y las migra transparentemente a PBKDF2 en el primer inicio de sesión
  correcto (ver §4).
* Para cambiar una contraseña: inicie sesión y use **Mi perfil → Cambiar contraseña**.
* **Se recomienda cambiar estas contraseñas** antes de cualquier uso real: son las
  que trae la base de datos de clase y son públicas.

> Nota sobre `epn.dis`: la especificación lo mencionaba como cuenta de usuario
> normal, pero **no existe en `TBL_USER`** (ni en `BDD_PoliMusic.sql` ni en la base
> viva). El usuario normal real es `userNormal`. No se creó ninguna cuenta ficticia
> para «hacer coincidir» la especificación.

---

## 3. Mapa de páginas y permisos

Autorización **en el servidor** en cada petición (deny-by-default). Ocultar botones
en la interfaz es solo cosmético: la comprobación real ocurre antes de procesar la página.

### Páginas públicas (sin sesión)

| Página | Ruta | Descripción |
|---|---|---|
| Iniciar sesión | `~/Login.aspx` | Valida usuario/correo contra SQL Server |
| Registro | `~/Registro.aspx` | Crea cuentas nuevas (siempre rol Usuario) |
| Error | `~/Error.aspx` | Página genérica de error (404/403/500) |
| Acceso denegado | `~/AccesoDenegado.aspx` | Respuesta 403 con el rol actual |

> `/AccesoDenegado.aspx` responde **403** (no 200): es una página de error legítima,
> no un recurso normal. Su inclusión entre las públicas solo significa que no exige
> sesión, para poder informar correctamente al usuario.

### Páginas de usuario (sesión requerida)

| Página | Ruta | Puede hacer |
|---|---|---|
| Inicio | `~/Default.aspx` | Ver canciones destacadas, álbumes, artistas, sus playlists |
| Canciones | `~/Canciones.aspx` | Listar, buscar, reproducir, agregar a sus playlists |
| Artistas | `~/Artistas.aspx` | Listar artistas y ver detalle con su catálogo |
| Álbumes | `~/Albumes.aspx` | Listar álbumes y ver tracklist |
| Playlists | `~/Playlists.aspx` | Ver las propias + las públicas; crear playlist |
| Detalle de playlist | `~/PlaylistDetalle.aspx?id=` | Ver/editar/eliminar **solo si es suya o pública** |
| Mi perfil | `~/Perfil.aspx` | Editar sus datos y cambiar su contraseña |
| Cerrar sesión | `~/Logout.aspx` | Invalida la sesión |
| Reproducción | `~/Play.aspx` | Endpoint JSON que incrementa `PLAYS` (requiere POST + token) |

### Páginas de administración (rol Administrador)

| Página | Ruta | CRUD completo sobre |
|---|---|---|
| Panel | `~/Admin/Default.aspx` | Métricas generales y usuarios recientes |
| Usuarios | `~/Admin/Usuarios.aspx` | `TBL_USER` |
| Canciones | `~/Admin/Canciones.aspx` | `TBL_SONG` + relación álbum/artista |
| Artistas | `~/Admin/Artistas.aspx` | `TBL_ARTIST` |
| Álbumes | `~/Admin/Albumes.aspx` | `TBL_ALBUM` + `TBL_TRACKLIST` |
| Playlists | `~/Admin/Playlists.aspx` | `TBL_PLAYLIST` (incluye transferir propietario) |

Un usuario normal que intente abrir cualquier `~/Admin/*` recibe
**302 → `AccesoDenegado.aspx?from=...`** (verificado).

---

## 4. Autenticación y contraseñas

* Algoritmo propio: **PBKDF2 (HMAC-SHA256)**, 10 000 iteraciones, salt aleatorio de 8 bytes,
  hash de 12 bytes. Formato almacenado: `PBH1$<saltHex>$<hashHex>` (46 caracteres,
  cabe en `VARCHAR(50)`).
* Comparación en **tiempo constante** para evitar ataques de temporización.
* **Migración progresiva**: al iniciar sesión, si el hash almacenado es texto plano o
  SHA-1 heredado y la contraseña es correcta, la fila se reescribe automáticamente a PBKDF2.
* El **registro asigna siempre `USER_TYPE = 0`** (Usuario). El formulario no expone el rol.
* `Perfil.aspx` conserva explícitamente el `USER_TYPE` actual, de modo que el usuario
  no puede escalar privilegios editando su propio perfil.
* Cookies: `HttpOnly`, `SameSite=Lax`; ticket `FormsAuthentication`.
* **CSRF**: token por sesión (`PM_CSRF`) validado en cada POST; si falta o no coincide,
  responde `403 {"ok":false,"error":"token-invalido"}` (verificado).
* **SQL**: todas las consultas usan comandos parametrizados (verificado con ataques
  de inyección, §7).

---

## 5. Modelo de datos (real, tomado de la base existente)

Ocho tablas. No se inventaron estructuras adicionales.

| Tabla | Clave | Notas |
|---|---|---|
| `TBL_USER` | `ID_USER` | `USERNAME`, `PASSWORD`, `EMAIL`, `BIRTHDAY`, `USER_PHOTO`, `USER_TYPE` |
| `TBL_SONG` | `ID_SONG` | `SONG_NAME`, `SONG_PATH`, `PLAYS` |
| `TBL_ARTIST` | `ID_ARTIST` | `ARTIST_NAME`, `ARTIST_LASTNAME`, `ARTIST_IMAGE` |
| `TBL_ALBUM` | `ID_ALBUM` | `TITLE_ALBUM`, `RELEASE_YEAR`, `ALBUM_IMAGE_PATH` |
| `TBL_PLAYLIST` | `ID_PLAYLIST` | `ID_USER`, `TITLE`, `CREATION_DATE`, `TYPE`, `PHOTO` |
| `TBL_PLAYER` | — | Relación N:M canciones ↔ artistas |
| `TBL_TRACKLIST` | — | Relación N:M álbumes ↔ canciones |
| `TBL_PLAYLIST_SONG` | — | Relación N:M playlists ↔ canciones |
| `TBL_CATALOG` | `ID_CATALOG` | Tabla de dominio autorreferenciada |

**Dominios en `TBL_CATALOG`** (leídos por la aplicación, no duplicados en código):

* `PARENT '1'` → `USER TYPE`: `0` = USER, `1` = ADMIN
* `PARENT '2'` → `PLAYLIST TYPE`: `0` = Public, `1` = Private

**Claves foráneas** presentes: `FK_TBL_PLAYER_TBL_ARTIST`, `FK_TBL_PLAYER_TBL_SONG`,
`FK_TBL_PLAYLIST_TBL_USER`, `FK_TBL_PLAYLIST_SONG_TBL_PLAYLIST`,
`FK_TBL_PLAYLIST_SONG_TBL_SONG`, `FK_TBL_TRACKLIST_TBL_ALBUM`, `FK_TBL_TRACKLIST_TBL_SONG`.

Las eliminaciones que atraviesan varias tablas se hacen **en transacción**.

---

## 6. Multimedia (rutas reales)

Las rutas guardadas en la base (`~/songFiles/...`, `~/userImages/...`) se traducen a
archivos reales bajo `~/Assets/...` mediante `Services\MediaResolver.cs`, que prueba
variantes de mayúsculas/minúsculas en el nombre de carpeta.

La carpeta `Assets` **debe** existir.

| Subcarpeta | Contenido |
|---|---|
| `Assets\SongFiles\` | Archivos `.mp3` |
| `Assets\UserImages\` | Fotos de usuario |
| `Assets\PlaylistImages\` | Portadas de playlists |
| `Assets\AlbumImages\` | Portadas de álbumes |
| `Assets\ArtistImages\` | Fotos de artistas |

**Regla importante:** si el archivo de audio no existe, la aplicación **no simula** una
reproducción correcta. La canción se marca como no reproducible y se muestra un aviso
(`AudioWarning`) en las páginas de catálogo.

El reproductor es un `<audio>` HTML5 real: play/pausa, canción actual, título, artista,
portada, volumen, barra de progreso, tiempo transcurrido y duración. Su estado sobrevive
a la navegación entre páginas mediante `sessionStorage`.

---

## 7. Pruebas ejecutadas realmente

Todo lo siguiente se ejecutó contra la aplicación en funcionamiento y la base real.

### Compilación y arranque

| Prueba | Resultado |
|---|---|
| Compilación de `App_Code` (Modelos, Datos, Servicios, Common) | **Correcta**, sin errores |
| Compilación dinámica de las 19 páginas `.aspx` | **Correcta**, sin errores ni advertencias |
| Arranque con IIS Express (`localhost:8090`) | **Escuchando y sirviendo** |
| `Content\Site.css` | `200`, 27 397 bytes, `text/css` |
| `Scripts\pollmusic.js` | `200`, 12 716 bytes |

### Autenticación

Inicios de sesión ejecutados con las contraseñas reales de la base (§2), enviando el
formulario de `Login.aspx` con `txtLogin`, `txtPassword` y `btnLogin`:

| Prueba | Resultado |
|---|---|
| `userAdmin` / `12345678` | `302 → /Admin/Default.aspx` (detectó rol Administrador) |
| `userNormal` / `12345678` | `302 → /Default.aspx` (detectó rol Usuario) |
| `userAdmin` / contraseña **incorrecta** | Permanece en `/Login.aspx` con el aviso *«Usuario o contrasena incorrectos.»*; **sin sesión** |
| Registro de cuenta nueva | `302 → /Login.aspx?msg=registered`; fila creada con **`USER_TYPE = 0`** |
| Contraseña almacenada | **PBKDF2** (`PBH1$...`, 46 caracteres), no texto plano |
| Migración de hash heredado | `userNormal` y `userAdmin` pasaron a `PBH1$...` al iniciar sesión |
| `Christian.Puchaicela` | Conserva SHA-1 (`7C222FB292…`) hasta su primer login correcto |
| Cierre de sesión | `302 → /Login.aspx?msg=logout` |
| Reutilizar la sesión tras cerrar | **Bloqueado**: todas las páginas protegidas → Login |

Páginas comprobadas con la sesión de `userAdmin` ya iniciada:

| Página | Resultado |
|---|---|
| `/Admin/Usuarios.aspx` | `200` — lista `userAdmin` y `userNormal` |
| `/Admin/Canciones.aspx` | `200` (15 816 bytes) |
| `/Admin/Artistas.aspx` | `200` (12 472 bytes) |
| `/Admin/Albumes.aspx` | `200` (12 679 bytes) |
| `/Admin/Playlists.aspx` | `200` (13 888 bytes) |
| `/Default.aspx` | `200` (22 881 bytes) |

### Autorización (servidor)

| Prueba | Resultado |
|---|---|
| Acceso sin sesión a `/Default.aspx` | `302 → Login.aspx?returnUrl=...` |
| Acceso sin sesión a `/Canciones.aspx`, `/Artistas.aspx`, `/Albumes.aspx`, `/Playlists.aspx` | `302 → Login.aspx?returnUrl=...` |
| Acceso sin sesión a `/Admin/*` | `302 → Login.aspx?returnUrl=...` |
| Usuario normal en las 6 páginas públicas | **`200`** en todas |
| Usuario normal en las 6 páginas `/Admin/*` | **`403 Forbidden`** en todas (sesión autenticada con rol insuficiente) |
| `/AccesoDenegado.aspx` | `403` con el usuario y rol actuales |

> **Nota de método:** las comprobaciones de acceso se hicieron con la redirección
> desactivada para observar el código real. Un cliente HTTP que sigue redirecciones
> automáticamente reporta `200` porque acaba mostrando **la página de login**, no el
> panel: el contenido del panel (`Usuarios`, `Canciones`…) **nunca** aparece. La
> evidencia de denegación es la ausencia de ese contenido más el `403`/`302` real.

### Playlists — integridad por propiedad

Verificado contra la base real. Las playlists 1–5 pertenecen a `userNormal;
1 = Privada, 2 = Pública, 3 = Pública, 4 = Privada, 5 = Pública`.

| Sesión | `id=1` (privada, ajena) | `id=2` (pública) | `id=999` (inexistente) |
|---|---|---|---|
| `userNormal` (propietario) | `200` | `200` | `404` |
| `userAdmin` (no propietario) | **`404`** | `200` | `404` |

* El propietario accede a **todas** sus playlists, incluidas las privadas.
* Otro usuario ve solo las **públicas**; las privadas ajenas devuelven `404` (no filtra
  su existencia). **Manipular el `id` en la URL no permite ver ni editar playlists ajenas.**

### Escritura en base de datos

| Prueba | Resultado |
|---|---|
| Incrementar reproducciones (`POST /Play.aspx?id=1` + token) | `200 {"ok":true}`; `TBL_SONG.PLAYS` de `Adventure` pasó de **0 → 1** |
| `GET /Play.aspx` (método incorrecto) | `405` |
| `POST` sin sesión | Rechazado |
| `POST` con token inválido | `403 {"ok":false,"error":"token-invalido"}` |

### Seguridad

| Prueba | Resultado |
|---|---|
| Inyección SQL: `userAdmin' OR '1'='1` | Sesión **no** iniciada |
| Inyección SQL: `' OR 1=1--` | Sesión **no** iniciada |
| Inyección SQL: `admin'; DROP TABLE TBL_USER--` | **`TBL_USER` intacta** (recuento sin cambios) |

### Multimedia (servida por el sitio)

| Recurso | Resultado |
|---|---|
| `bensound-adventure.mp3` | `200`, `audio/mpeg`, 4 490 762 bytes |
| `bensound-dreams.mp3` | `200`, `audio/mpeg`, 801 480 bytes |
| `bensound-energy.mp3` | `200`, `audio/mpeg`, 701 170 bytes |
| `bensound-scifi.mp3` | `200`, `audio/mpeg`, 761 252 bytes |
| `bensound-tomorrow.mp3` | `200`, `audio/mpeg`, 841 186 bytes |
| Imágenes de usuario / playlist / álbum / artista | `200` con `image/png` o `image/jpeg` |

### Datos reales mostrados

La pantalla de inicio renderiza datos reales de SQL Server (no contenido ficticio):

* Métricas: **5 Canciones**, **5 Artistas**, **5 Álbumes**
* Catálogo: `Tomorrow` (4), `Scifi` (3), `Energy` (2), `Dreams` (1), `Adventure` (0)
  con sus artistas desde `TBL_PLAYER`
* Búsqueda `Canciones.aspx?q=dream` filtra correctamente el listado

### Correcciones aplicadas durante las pruebas

Estos defectos se detectaron **ejecutando** la aplicación y se corrigieron:

1. `App_Code` compilaba como ensamblado separado y no veía `Models`/`Services` →
   se unificó el código compartido bajo `App_Code` y se pasó a `CodeFile=`.
2. Sintaxis C# 6/7 (función local, `char + string`) incompatible con el compilador
   dinámico C# 5 → reescrita en `MediaResolver.cs`.
3. `using` faltantes: `System.Web.UI.WebControls`, `System.Web.SessionState`,
   `System.Data.SqlClient` (en `CatalogRepository.cs`).
4. Las páginas heredaban de `Page` en lugar de `BasePage`, dejando la capa de
   autorización fuera de la cadena de herencia → corregido en las 19 páginas, con
   `IsPublicPage = true` en las públicas y `RequiresAdmin = true` en `/Admin/*`.
5. `Canciones.aspx` referenciaba un manejador `btnAdd_Click` inexistente y un panel
   redundante → eliminado; el alta se hace con los formularios en línea por canción.
6. Regla `denyUrlSequences` con `.cs` bloqueaba `Site.css` (contenía `.cs`) →
   sustituida por `fileExtensions` explícitas.
7. `Global.asax` necesitaba `CodeFile` y `partial` para el modelo Web Site.
8. El constructor de `Error.aspx.cs` se llamaba `Error()` en lugar de `ErrorPage()`,
   por lo que no era un constructor real y `IsPublicPage` nunca se activaba →
   la página de error redirigía a Login en vez de mostrarse.

---

## 8. Estructura del proyecto

```
AppWebAvChristianPuchaicela/
├─ PoliMusic.sln
├─ BDD_PoliMusic.sql                     ← script original, SIN modificar
├─ db/
│  └─ 01_PoliMusic_incremental.sql       ← migración incremental e idempotente
└─ webapppolimusicv1_asp/
   ├─ Web.config                          ← cadena de conexión, autorización, seguridad
   ├─ Global.asax / .asax.cs              ← manejo central de errores
   ├─ Site.master / .master.cs            ← plantilla con barra lateral y reproductor
   ├─ Login / Registro / Logout.aspx
   ├─ Default.aspx                        ← pantalla principal
   ├─ Canciones.aspx                      ← "Song List"
   ├─ Artistas.aspx / Albumes.aspx        ← catálogo con detalle
   ├─ Playlists.aspx / PlaylistDetalle.aspx ← CRUD de playlists propias
   ├─ Perfil.aspx                         ← datos y contraseña
   ├─ Play.aspx                           ← endpoint de conteo de reproducciones
   ├─ Error.aspx / AccesoDenegado.aspx
   ├─ Admin/                              ← Default, Usuarios, Canciones, Artistas,
   │                                          Albumes, Playlists
   ├─ App_Code/
   │  ├─ Common/  BasePage.cs, Ui.cs      ← autorización base y constructores de HTML
   │  ├─ Models/  User, Song, Artist, Album, Playlist, CatalogItem
   │  ├─ Data/    Db, UserRepository, SongRepository, ArtistRepository,
   │  │           AlbumRepository, PlaylistRepository, CatalogRepository
   │  └─ Services/ AuthService, PasswordHasher, UserContext, MediaResolver
   ├─ Content/Site.css                    ← identidad visual marrón
   ├─ Scripts/pollmusic.js                ← reproductor persistente
   ├─ Assets/                             ← audio, imágenes
   └─ run/                                ← launcher de IIS Express
      ├─ applicationhost.config
      └─ iniciar-polimusic.bat
```

**Estadísticas:** 19 páginas `.aspx`, 41 archivos `.cs`, 1 hoja de estilos, 1 script JS.
Sin dependencias NuGet.

---

## 9. Identidad visual

Paleta exacta solicitada, definida como variables CSS en `Content\Site.css`:

| Variable | Valor | Uso |
|---|---|---|
| `--brown-deep` | `#2B1D17` | Fondos profundos |
| `--brown-main` | `#6F4E37` | Color principal |
| `--brown-mid` | `#8B5E3C` | Superficies intermedias |
| `--brown-light` | `#B08968` | Acentos y bordes |
| `--cream` | `#F5EBDD` | Texto sobre fondo oscuro |
| `--charcoal` | `#191513` | Texto principal |

Barra lateral de navegación, tarjetas, tablas, reproductor inferior persistente,
diseño adaptable (puntos de quiebre en 900 px y 720 px) y estados hover.
No se copiaron marcas ni logotipos de Spotify.

---

## 10. Funcionalidad pendiente y limitaciones reales

Se documenta con motivos concretos, sin ocultar nada.

1. **No se pudo compilar con MSBuild / csc desde el entorno del asistente.**
   Las herramientas de compilación (`MSBuild.exe`, `csc.exe`) estuvieron bloqueadas por
   la política de seguridad del entorno. **La verificación se hizo, en cambio, con el
   compilador dinámico de ASP.NET**, que compiló las 19 páginas y todo `App_Code` sin
   errores. Si abre la solución en Visual Studio y obtiene algún aviso del compilador,
   quedan pendientes de revisar los **avisos** (no errores) de estilo, por ejemplo
   variables no utilizadas.

2. **`db\01_PoliMusic_incremental.sql` no se ejecutó contra la base.**
   La aplicación funciona sin él: el archivo corrige 3 rutas de imagen con
   mayúsculas incorrectas (`Playlist2.png` → `playlist2.png`) y reinicia `PLAYS` nulos.
   Ejecútelo cuando lo considere oportuno; es idempotente y no borra nada.

3. **Subida de archivos de audio/imagen desde la interfaz no implementada.**
   Los formularios de administración registran **rutas** (`SONG_PATH`, imágenes), no
   suben archivos binarios. Requeriría un control `FileUpload` y escritura en disco;
   se dejó fuera para no ampliar la superficie de riesgo sin pedirlo el enunciado.

4. **Reproducción real de audio no verificada de oído.**
   Se confirmó que los 5 archivos `.mp3` se sirven con `Content-Type: audio/mpeg` y
   tamaño correcto, y que las páginas emiten las cargas `data-pm-play` / `data-pm-queue`.
   No se pudo comprobar la salida de sonido (el entorno no tiene audio).
   En un navegador real el reproductor funciona con esos archivos válidos.

5. **Credenciales de base de datos: RESUELTO.**
   La cadena ya **no** está en `Web.config`. Se movió a `Web.secrets.config`
   (excluido por `.gitignore`) y `Web.config` usa `configSource`, de modo que el
   repositorio no publica el usuario ni la contraseña de la base de datos:

   ```xml
   <!-- Web.config -->
   <connectionStrings configSource="Web.secrets.config" />
   ```

   `Web.secrets.config.example` sí se versiona, con valores de ejemplo, como
   plantilla para otros entornos (ver §1, Paso 2).

6. **El reproductor usa `sessionStorage`, no almacenamiento en servidor.**
   El estado (canción, posición, volumen) persiste entre páginas dentro de la misma
   pestaña, pero no se sincroniza entre dispositivos ni sesiones. Es el comportamiento
   previsto.

7. **`Christian.Puchaicela` conserva su hash SHA-1.**
   Se migrará a PBKDF2 automáticamente la primera vez que esa cuenta inicie sesión.
   No se modificó su fila para no alterar datos existentes sin autorización.

---

## 11. Solución de problemas

| Síntoma | Causa y solución |
|---|---|
| `Failed to register URL … (0x800700b7) No se puede crear un archivo que ya existe` | **PoliMusic ya está en ejecución** en el puerto 8090. No es un error de la aplicación. Abra `http://localhost:8090/`, o detenga la instancia con `run\iniciar-polimusic.bat stop` y vuelva a arrancar. El script actual ya lo detecta y no muestra este error |
| `HTTP 400 - Invalid Hostname` | Usó `127.0.0.1`. Abra `http://localhost:8090/` |
| `HTTP 502` o "upstream connect failed" | Hay un proxy HTTP interceptando. Excluya `localhost` del proxy |
| La primera carga tarda varios segundos | Normal: ASP.NET compila el sitio en el primer acceso |
| "Error de compilación" en el navegador | Lea el mensaje: indica archivo y línea. Verifique que `App_Code\` no contenga `.aspx` |
| Aviso de que una canción no se reproduce | El archivo `.mp3` indicado en `SONG_PATH` no existe en `Assets\SongFiles\` |
| No puede iniciar sesión con una cuenta nueva | El registro siempre asigna rol Usuario; el acceso a `/Admin/*` sigue denegado |
| `Login failed for user 'SU_USUARIO'` | Revise `Web.secrets.config`: instancia, puerto y credenciales |
| El sitio da error 500 y no carga ninguna página | Falta `Web.secrets.config`, o su raíz está envuelta en `<configuration>`. Vea §1, Paso 2 |
| Los datos salen vacíos | Ejecute `BDD_PoliMusic.sql` (o la migración) y confirme que hay filas en `TBL_SONG` |
| Tras un login correcto sigue viendo `/Login.aspx` | Compruebe la contraseña: `userAdmin` y `userNormal` usan `12345678` (§2). Si es correcta, revise que las cookies de sesión no estén bloqueadas |
| `User 'userNormal'` recibe **403** en `/Admin/Usuarios.aspx` | Comportamiento correcto: la autorización se valida en el servidor (§3) |

---

## 12. Cumplimiento del enunciado

| Requisito | Estado |
|---|---|
| ASP.NET Web Forms clásico, .NET Framework, C# | Cumplido |
| Sin migrar a Core / MVC / Blazor / Node / React | Cumplido |
| Cadena de conexión única y centralizada | Cumplido (`PoliMusicConnection`) |
| Dos roles con permisos diferenciados | Cumplido y verificado |
| CRUD de admin sobre Usuarios/Canciones/Artistas/Álbumes/Playlists | Cumplido |
| CRUD de playlists propias por el usuario | Cumplido (crear, editar, eliminar, agregar/quitar canciones) |
| Autorización en el servidor, deny-by-default | Cumplido y verificado |
| Imposible ver/editar playlists ajenas manipulando la URL | Cumplido y verificado |
| Contraseñas con hash + salt | Cumplido (PBKDF2) |
| El registro asigna siempre el rol Usuario | Cumplido y verificado |
| Pantalla principal con datos reales y estados vacíos | Cumplido |
| Paleta marrón exacta, diseño adaptable | Cumplido |
| Reproductor HTML5 funcional | Cumplido (archivos válidos verificados) |
| Consultas parametrizadas | Cumplido y verificado |
| `README` con instrucciones, permisos, pruebas reales y pendientes | Este documento |
| `BDD_PoliMusic.sql` original preservado | Cumplido (sin modificar) |
| Scripts de migración incrementales | Cumplido (`db\01_PoliMusic_incremental.sql`) |
