using System;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;

namespace WebAppPoliMusicV2.Util
{
    /// <summary>
    /// Guardado de archivos subidos (fotos de usuario, portadas y mp3).
    ///
    /// Reemplaza a FileManagement: ya no hereda de Page (era un error de
    /// diseno: un servicio no debe ser una pagina) y valida extension,
    /// tipo MIME y tamano antes de escribir en disco. Los nombres de archivo
    /// se sanean para impedir rutas arbitrarias.
    /// </summary>
    public static class FileStorage
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const int MaxImageBytes = 5 * 1024 * 1024;   // 5 MB

        /// <summary>
        /// Guarda una imagen y devuelve la ruta relativa guardada
        /// (por ejemplo ~/UserImages/christian.jpg) o null si no se subio
        /// ningun archivo. En caso de error devuelve el mensaje en <paramref name="error"/>.
        /// </summary>
        public static string SaveImage(FileUpload upload, string folder, string baseName, out string error)
        {
            error = null;
            if (upload == null || !upload.HasFile)
            {
                return null;
            }
            return SaveImage(upload.PostedFile, folder, baseName, out error);
        }

        /// <summary>
        /// Guarda una imagen recibida como HttpPostedFile (formularios HTML
        /// planos con &lt;input type="file"&gt;).
        /// </summary>
        public static string SaveImage(HttpPostedFile file, string folder, string baseName, out string error)
        {
            error = null;
            if (file == null || file.ContentLength <= 0)
            {
                return null;
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (Array.IndexOf(AllowedImageExtensions, extension) < 0)
            {
                error = "Formato no permitido. Use JPG, PNG, GIF o WEBP.";
                return null;
            }
            if (file.ContentLength > MaxImageBytes)
            {
                error = "La imagen supera el limite de 5 MB.";
                return null;
            }

            try
            {
                string serverFolder = HttpContext.Current.Server.MapPath(folder);
                if (!Directory.Exists(serverFolder))
                {
                    Directory.CreateDirectory(serverFolder);
                }
                string safeName = SanitizeFileName(baseName);
                if (safeName.Length == 0)
                {
                    safeName = Guid.NewGuid().ToString("N");
                }
                string fileName = safeName + extension;
                file.SaveAs(Path.Combine(serverFolder, fileName));
                return folder.TrimEnd('/') + "/" + fileName;
            }
            catch (Exception ex)
            {
                error = "No se pudo guardar la imagen: " + ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Guarda un mp3 y devuelve su ruta relativa. El tamano maximo se lee
        /// de appSettings/maxSongLength (50 MB por defecto).
        /// </summary>
        public static string SaveSong(FileUpload upload, string folder, string baseName, out string error)
        {
            error = null;
            if (upload == null || !upload.HasFile)
            {
                error = "Seleccione un archivo de audio.";
                return null;
            }
            return SaveSong(upload.PostedFile, folder, baseName, out error);
        }

        /// <summary>Guarda un mp3 recibido como HttpPostedFile.</summary>
        public static string SaveSong(HttpPostedFile file, string folder, string baseName, out string error)
        {
            error = null;
            if (file == null || file.ContentLength <= 0)
            {
                error = "Seleccione un archivo de audio.";
                return null;
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".mp3")
            {
                error = "Solo se permiten archivos .mp3.";
                return null;
            }

            int maxBytes = 52428800;
            string configured = DAL.Db.Setting("maxSongLength", "52428800");
            int parsed;
            if (int.TryParse(configured, out parsed) && parsed > 0)
            {
                maxBytes = parsed;
            }
            if (file.ContentLength > maxBytes)
            {
                error = "El archivo supera el limite de " + (maxBytes / 1024 / 1024) + " MB.";
                return null;
            }

            try
            {
                string serverFolder = HttpContext.Current.Server.MapPath(folder);
                if (!Directory.Exists(serverFolder))
                {
                    Directory.CreateDirectory(serverFolder);
                }
                string safeName = SanitizeFileName(baseName);
                if (safeName.Length == 0)
                {
                    safeName = Guid.NewGuid().ToString("N");
                }
                string fileName = safeName + extension;
                file.SaveAs(Path.Combine(serverFolder, fileName));
                return folder.TrimEnd('/') + "/" + fileName;
            }
            catch (Exception ex)
            {
                error = "No se pudo guardar el audio: " + ex.Message;
                return null;
            }
        }

        /// <summary>Copia la imagen por defecto con un nombre nuevo y devuelve su ruta.</summary>
        public static string CopyDefaultImage(string folder, string defaultFile, string baseName)
        {
            try
            {
                string serverFolder = HttpContext.Current.Server.MapPath(folder);
                string source = Path.Combine(serverFolder, defaultFile);
                if (!File.Exists(source))
                {
                    return null;
                }
                string safeName = SanitizeFileName(baseName);
                if (safeName.Length == 0)
                {
                    return null;
                }
                string fileName = safeName + Path.GetExtension(defaultFile);
                File.Copy(source, Path.Combine(serverFolder, fileName), true);
                return folder.TrimEnd('/') + "/" + fileName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Elimina un archivo del sitio a partir de su ruta (~/...). Solo actua
        /// dentro de las carpetas de recursos, nunca fuera de la aplicacion.
        /// </summary>
        public static void DeleteIfExists(string virtualPath)
        {
            string resolved = MediaResolver.Resolve(virtualPath);
            if (resolved == null)
            {
                return;
            }
            try
            {
                string physical = HttpContext.Current.Server.MapPath(resolved);
                if (File.Exists(physical))
                {
                    File.Delete(physical);
                }
            }
            catch (Exception)
            {
                // El borrado del archivo es secundario: nunca debe romper la operacion.
            }
        }

        /// <summary>Quita caracteres invalidos y espacios de un nombre de archivo.</summary>
        public static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            char[] invalid = Path.GetInvalidFileNameChars();
            System.Text.StringBuilder sb = new System.Text.StringBuilder(name.Length);
            foreach (char c in name.Trim())
            {
                if (Array.IndexOf(invalid, c) >= 0 || c == ' ')
                {
                    continue;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
