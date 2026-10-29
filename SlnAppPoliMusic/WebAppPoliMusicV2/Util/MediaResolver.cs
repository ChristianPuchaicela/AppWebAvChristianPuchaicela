using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace WebAppPoliMusicV2.Util
{
    /// <summary>
    /// Traduce las rutas guardadas en la base de datos (por ejemplo
    /// '~/songFiles/bensound-adventure.mp3' o '~/ArtistImages/artist1.jpg')
    /// a rutas web realmente utilizables por el navegador.
    ///
    /// La base usa nombres de carpeta con mayusculas incoherentes
    /// (songFiles, albumImages, ArtistImages, userImages, playlistImages),
    /// mientras que en disco las carpetas del proyecto son SongFiles,
    /// AlbumImages, ArtistImages, UserImages y PlaylistImages. Por eso la
    /// resolucion prueba la ruta exacta, la version capitalizada y, como
    /// ultimo recurso, busca la carpeta y el archivo sin distinguir
    /// mayusculas. Asi la base de datos no necesita modificarse.
    ///
    /// Si el archivo no existe devuelve null y la interfaz muestra un
    /// marcador de posicion: nunca se simula una reproduccion inexistente.
    /// </summary>
    public static class MediaResolver
    {
        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<string, string> FolderCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static string RootPath
        {
            get
            {
                HttpContext ctx = HttpContext.Current;
                return ctx != null
                    ? ctx.Server.MapPath("~/")
                    : AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        /// <summary>Convierte una ruta de la BD en ruta web (~/...) o null.</summary>
        public static string Resolve(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                return null;
            }

            string relative = dbPath.Replace('\\', '/').Trim();
            if (relative.StartsWith("~/", StringComparison.Ordinal))
            {
                relative = relative.Substring(2);
            }
            relative = relative.TrimStart('/');

            int slash = relative.IndexOf('/');
            if (slash <= 0)
            {
                return null;
            }

            string folder = relative.Substring(0, slash);
            string file = relative.Substring(slash + 1).Trim();
            if (file.Length == 0 || file.IndexOf("..", StringComparison.Ordinal) >= 0 ||
                folder.IndexOf("..", StringComparison.Ordinal) >= 0)
            {
                return null;
            }

            string root = RootPath;
            if (string.IsNullOrEmpty(root))
            {
                return null;
            }

            // 1) Ruta tal cual la guarda la base.
            string direct = Path.Combine(root, folder, file);
            if (File.Exists(direct))
            {
                return ToUrl("~/" + folder + "/" + file);
            }

            // 2) Carpeta con inicial mayuscula (songFiles -> SongFiles).
            string capitalized = char.ToUpperInvariant(folder[0]) + folder.Substring(1);
            string alt = Path.Combine(root, capitalized, file);
            if (File.Exists(alt))
            {
                return ToUrl("~/" + capitalized + "/" + file);
            }

            // 3) Busqueda sin distinguir mayusculas, con cache de carpetas.
            string realFolder = ResolveFolderName(root, folder);
            if (realFolder == null)
            {
                return null;
            }
            string realFile = ResolveFileName(Path.Combine(root, realFolder), file);
            if (realFile == null)
            {
                return null;
            }
            return ToUrl("~/" + realFolder + "/" + realFile);
        }

        /// <summary>
        /// Convierte una ruta logica (~/carpeta/archivo) en una URL que el
        /// navegador puede pedir de verdad.
        ///
        /// Esto es imprescindible: el caracter ~/ solo lo entiende ASP.NET
        /// (controles de servidor, ResolveUrl). Si se escribe tal cual en un
        /// &lt;img src="~/..."&gt; o en el JSON del reproductor, el navegador
        /// lo interpreta como una carpeta llamada "~" y devuelve 404.
        ///
        /// VirtualPathUtility.ToAbsolute respeta el directorio virtual de la
        /// aplicacion: con la app en la raiz devuelve /UserImages/default.jpg
        /// y con la app en /PoliMusic devuelve /PoliMusic/UserImages/default.jpg.
        /// </summary>
        public static string ToUrl(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                return virtualPath;
            }

            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Request == null)
            {
                // Tiempo de diseno o fuera de una peticion: se deja la ruta logica.
                return virtualPath;
            }

            try
            {
                return VirtualPathUtility.ToAbsolute(virtualPath);
            }
            catch (Exception)
            {
                return virtualPath;
            }
        }

        /// <summary>Indica si la ruta de la base tiene un archivo real detras.</summary>
        public static bool Exists(string dbPath)
        {
            return Resolve(dbPath) != null;
        }

        /// <summary>Ruta web de una portada, con respaldo en otra ruta.</summary>
        public static string ResolveWithFallback(string dbPath, string fallbackDbPath)
        {
            return Resolve(dbPath) ?? Resolve(fallbackDbPath);
        }

        private static string ResolveFolderName(string root, string folder)
        {
            string cached;
            lock (SyncRoot)
            {
                if (FolderCache.TryGetValue(folder, out cached))
                {
                    return string.IsNullOrEmpty(cached) ? null : cached;
                }
            }

            string found = null;
            try
            {
                foreach (string dir in Directory.GetDirectories(root))
                {
                    string name = Path.GetFileName(dir);
                    if (string.Equals(name, folder, StringComparison.OrdinalIgnoreCase))
                    {
                        found = name;
                        break;
                    }
                }
            }
            catch (IOException)
            {
                found = null;
            }
            catch (UnauthorizedAccessException)
            {
                found = null;
            }

            lock (SyncRoot)
            {
                FolderCache[folder] = found ?? string.Empty;
            }
            return found;
        }

        private static string ResolveFileName(string folderPath, string file)
        {
            string direct = Path.Combine(folderPath, file);
            if (File.Exists(direct))
            {
                return file;
            }
            try
            {
                foreach (string candidate in Directory.GetFiles(folderPath))
                {
                    string name = Path.GetFileName(candidate);
                    if (string.Equals(name, file, StringComparison.OrdinalIgnoreCase))
                    {
                        return name;
                    }
                }
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            return null;
        }

        /// <summary>Ruta absoluta en disco de una ruta web (~/...).</summary>
        public static string MapPath(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                return null;
            }
            HttpContext ctx = HttpContext.Current;
            return ctx != null
                ? ctx.Server.MapPath(virtualPath)
                : Path.Combine(RootPath, virtualPath.Replace("~/", string.Empty).Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
