using System;
using System.IO;
using System.Web;
using WebAppPoliMusicV1.Data;

namespace WebAppPoliMusicV1.Services
{
    /// <summary>
    /// Traduce las rutas guardadas en la base de datos (por ejemplo
    /// '~/songFiles/bensound-adventure.mp3' o '~/userImages/root.png')
    /// a rutas web utilizables por el navegador.
    ///
    /// La base de datos usa nombres de carpeta en minuscula inicial
    /// (songFiles, userImages, playlistImages, albumImages, ArtistImages).
    /// En disco los recursos reales viven bajo ~/Assets/UserImages,
    /// ~/Assets/PlaylistImages y ~/Assets/SongFiles. El resolutor prueba
    /// variantes de mayusculas/minusculas para que las rutas de la base de
    /// datos funcionen sin modificarla.
    ///
    /// Si no encuentra el archivo devuelve null y el llamador muestra un
    /// marcador de posicion: nunca se simula una reproduccion inexistente.
    /// </summary>
    public static class MediaResolver
    {
        private static readonly string[] FolderCandidates =
        {
            "Assets/{0}",
            "Assets/{1}",
            "SongFilesAndImages_resources/{0}",
            "SongFilesAndImages_resources/{1}"
        };

        private static string MediaRoot
        {
            get { return Db.Setting("MediaRoot", "~/Assets"); }
        }

        /// <summary>
        /// Convierte una ruta de la BD en ruta web (~/...) o null si el archivo no existe.
        /// </summary>
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
            string file = relative.Substring(slash + 1);
            if (file.Length == 0 || file.IndexOf("..", StringComparison.Ordinal) >= 0)
            {
                return null;
            }

            string physical = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/")
                : AppDomain.CurrentDomain.BaseDirectory;

            // Variantes del nombre de carpeta: tal cual, capitalizado y con
            // y sin la 's' final (songFiles -> SongFiles).
            string[] folderVariants = BuildFolderVariants(folder);

            foreach (string template in new[] { "{0}", "{1}" })
            {
                foreach (string candidateFolder in folderVariants)
                {
                    string rel = template == "{0}"
                        ? string.Concat(candidateFolder, "/", file)
                        : string.Concat(MediaRootTrimmed(), "/", candidateFolder, "/", file);

                    string full = Path.Combine(physical, rel.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(full))
                    {
                        return "~/" + rel.Replace('\\', '/');
                    }
                }
            }

            return null;
        }

        /// <summary>Devuelve una ruta web solo si el archivo existe; si no, null.</summary>
        public static bool Exists(string dbPath)
        {
            return Resolve(dbPath) != null;
        }

        private static string MediaRootTrimmed()
        {
            string root = MediaRoot.Replace('\\', '/').Trim();
            if (root.StartsWith("~/", StringComparison.Ordinal))
            {
                root = root.Substring(2);
            }
            return root.Trim('/');
        }

        private static string[] BuildFolderVariants(string folder)
        {
            var list = new System.Collections.Generic.List<string>();
            string capitalized = folder.Length > 0
                ? char.ToUpperInvariant(folder[0]).ToString() + folder.Substring(1)
                : folder;

            AddIfNew(list, folder);
            AddIfNew(list, capitalized);

            // 'songFiles' -> 'SongFiles'; 'userImages' -> 'UserImages'
            if (capitalized.EndsWith("Files", StringComparison.OrdinalIgnoreCase))
            {
                AddIfNew(list, capitalized.Substring(0, capitalized.Length - 5) + "Files");
            }
            if (capitalized.EndsWith("Images", StringComparison.OrdinalIgnoreCase))
            {
                AddIfNew(list, capitalized);
            }

            return list.ToArray();
        }

        /// <summary>Agrega un valor a la lista si no existe ya (sin distinguir mayusculas).</summary>
        private static void AddIfNew(System.Collections.Generic.List<string> list, string value)
        {
            foreach (string existing in list)
            {
                if (string.Equals(existing, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            list.Add(value);
        }

        /// <summary>
        /// Ruta web de la portada, con respaldo en la imagen por defecto del tipo
        /// correspondiente. Devuelve null solo si no hay ningun recurso disponible.
        /// </summary>
        public static string ResolveWithFallback(string dbPath, string fallbackDbPath)
        {
            string resolved = Resolve(dbPath);
            if (resolved != null)
            {
                return resolved;
            }
            return Resolve(fallbackDbPath);
        }
    }
}
