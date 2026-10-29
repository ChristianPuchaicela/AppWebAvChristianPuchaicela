using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Genera el HTML de los componentes compartidos (tarjetas, filas de tabla,
    /// estados vacios, avisos). Centraliza el escapado y la resolucion de rutas
    /// multimedia para que todas las paginas se vean y se comporten igual.
    /// </summary>
    public static class Ui
    {
        public const string IconPlay =
            "<svg viewBox=\"0 0 24 24\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M8 5v14l11-7z\"/></svg>";

        public const string IconPause =
            "<svg viewBox=\"0 0 24 24\" fill=\"currentColor\" aria-hidden=\"true\"><rect x=\"6\" y=\"4\" width=\"4\" height=\"16\" rx=\"1\"/><rect x=\"14\" y=\"4\" width=\"4\" height=\"16\" rx=\"1\"/></svg>";

        // ------------------------------------------------------- escapado ---

        public static string E(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }

        public static string A(string value)
        {
            return HttpUtility.HtmlAttributeEncode(value ?? string.Empty);
        }

        private static string JsonStr(string value)
        {
            return "\"" + HttpUtility.JavaScriptStringEncode(value ?? string.Empty, false) + "\"";
        }

        // ------------------------------------------------------ identidad ---

        /// <summary>Iniciales para el marcador de posicion de una portada.</summary>
        public static string InitialsOf(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "\u266A";
            }
            string t = text.Trim();
            string[] parts = t.Split(new[] { ' ', '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpperInvariant();
            }
            return t.Substring(0, Math.Min(2, t.Length)).ToUpperInvariant();
        }

        /// <summary>Contenido de una caja de portada: imagen o iniciales.</summary>
        public static string Art(string url, string fallbackText)
        {
            if (!string.IsNullOrEmpty(url))
            {
                return "<img src=\"" + A(url) + "\" alt=\"\" loading=\"lazy\" " +
                       "onerror=\"this.style.display='none';this.parentNode.classList.add('is-empty')\">";
            }
            return "<span class=\"pm-art-initials\">" + E(InitialsOf(fallbackText)) + "</span>";
        }

        // ------------------------------------------------------ reproductor -

        /// <summary>Serializa una cancion para el reproductor en un atributo data.</summary>
        public static string PlayPayload(Song song, string artistLabel, string artUrl)
        {
            if (song == null)
            {
                return null;
            }
            string src = MediaResolver.Resolve(song.SongPath);
            if (src == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"id\":").Append(song.IdSong);
            sb.Append(",\"src\":").Append(JsonStr(src));
            sb.Append(",\"title\":").Append(JsonStr(song.SongName));
            sb.Append(",\"artist\":").Append(JsonStr(artistLabel ?? "Sin artista"));
            sb.Append(",\"art\":").Append(JsonStr(artUrl));
            sb.Append(",\"initials\":").Append(JsonStr(InitialsOf(song.SongName)));
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>Cola de reproduccion completa para los botones siguiente/anterior.</summary>
        public static string PlayQueue(IEnumerable<Song> songs, string artUrl)
        {
            if (songs == null)
            {
                return "[]";
            }
            StringBuilder sb = new StringBuilder("[");
            bool first = true;
            foreach (Song s in songs)
            {
                string payload = PlayPayload(s, s.ArtistLabel, artUrl);
                if (payload == null)
                {
                    continue;
                }
                if (!first)
                {
                    sb.Append(',');
                }
                sb.Append(payload);
                first = false;
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static string PlayButton(string payload, string queueJson, string cssClass, string label)
        {
            if (payload == null)
            {
                return "<span class=\"" + cssClass + " is-disabled\" title=\"Sin archivo de audio disponible\" " +
                       "aria-disabled=\"true\">" + IconPlay + "</span>";
            }
            return "<button type=\"button\" class=\"" + cssClass + "\" data-pm-play=\"" + A(payload) +
                   "\" data-pm-queue=\"" + A(queueJson) + "\" aria-label=\"Reproducir " + A(label) + "\">" +
                   IconPlay + "</button>";
        }

        // --------------------------------------------------------- tarjetas -

        public static string SongCard(Song song, string artUrl, string queueJson)
        {
            string payload = PlayPayload(song, song.ArtistLabel, artUrl);
            return "<article class=\"pm-card\">"
                 + "<div class=\"pm-card-art\">" + Art(artUrl, song.SongName)
                 + PlayButton(payload, queueJson, "pm-card-play", song.SongName)
                 + "</div>"
                 + "<div class=\"pm-card-title\">" + E(song.SongName) + "</div>"
                 + "<div class=\"pm-card-sub\">" + E(song.ArtistLabel ?? "Sin artista") + "</div>"
                 + "</article>";
        }

        public static string AlbumCard(Album album, string artUrl, string detailUrl)
        {
            return "<article class=\"pm-card\">"
                 + "<a class=\"pm-card-link\" href=\"" + A(detailUrl) + "\" aria-label=\"" + A(album.TitleAlbum) + "\"></a>"
                 + "<div class=\"pm-card-art\">" + Art(artUrl, album.TitleAlbum) + "</div>"
                 + "<div class=\"pm-card-title\">" + E(album.TitleAlbum) + "</div>"
                 + "<div class=\"pm-card-sub\">" + E(album.ReleaseLabel) + " &middot; " +
                   album.SongCount + " canci&oacute;n(es)</div>"
                 + "</article>";
        }

        public static string ArtistCard(Artist artist, string artUrl, string detailUrl)
        {
            return "<article class=\"pm-card pm-card-center\">"
                 + "<a class=\"pm-card-link\" href=\"" + A(detailUrl) + "\" aria-label=\"" + A(artist.FullName) + "\"></a>"
                 + "<div class=\"pm-card-art round\">" + Art(artUrl, artist.FullName) + "</div>"
                 + "<div class=\"pm-card-title\">" + E(artist.FullName) + "</div>"
                 + "<div class=\"pm-card-sub\">" + artist.SongCount + " canci&oacute;n(es)</div>"
                 + "</article>";
        }

        public static string PlaylistCard(Playlist playlist, string artUrl, string detailUrl)
        {
            string badge = playlist.IsPrivate
                ? "<span class=\"pm-badge pm-badge-private\">Privada</span>"
                : "<span class=\"pm-badge pm-badge-public\">P&uacute;blica</span>";

            return "<article class=\"pm-card\">"
                 + "<a class=\"pm-card-link\" href=\"" + A(detailUrl) + "\" aria-label=\"" + A(playlist.Title) + "\"></a>"
                 + "<div class=\"pm-card-art\">" + Art(artUrl, playlist.Title) + "</div>"
                 + "<div class=\"pm-card-title\">" + E(playlist.Title) + "</div>"
                 + "<div class=\"pm-card-sub\">" + playlist.SongCount + " canci&oacute;n(es)</div>"
                 + "<div class=\"pm-card-foot\">" + badge + "</div>"
                 + "</article>";
        }

        // ------------------------------------------------------------ filas -

        /// <summary>
        /// Fila de tabla de una cancion. <paramref name="extraActions"/> recibe
        /// los botones propios de la pagina (agregar a playlist, editar, borrar).
        /// </summary>
        public static string SongRow(Song song, int index, string queueJson, string extraActions)
        {
            string payload = PlayPayload(song, song.ArtistLabel, null);
            return "<tr>"
                 + "<td data-label=\"#\"><div class=\"pm-song-row\"><span class=\"pm-song-index\">" +
                   index + "</span>" + PlayButton(payload, queueJson, "pm-play-mini", song.SongName) +
                   "</div></td>"
                 + "<td data-label=\"Canci&oacute;n\"><strong>" + E(song.SongName) + "</strong></td>"
                 + "<td data-label=\"Artista\">" + E(song.ArtistLabel ?? "Sin artista") + "</td>"
                 + "<td data-label=\"&Aacute;lbum\">" +
                   (string.IsNullOrEmpty(song.AlbumTitle) ? "&mdash;" : E(song.AlbumTitle)) + "</td>"
                 + "<td data-label=\"Reproducciones\" class=\"pm-plays\">" + song.PlayCount + "</td>"
                 + "<td data-label=\"Acciones\"><div class=\"pm-row-actions\">" +
                   (extraActions ?? string.Empty) + "</div></td>"
                 + "</tr>";
        }

        // --------------------------------------------------------- bloques ---

        public static string EmptyState(string title, string message)
        {
            return "<div class=\"pm-empty\">"
                 + "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.4\" " +
                   "stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">"
                 + "<path d=\"M9 18V5l10-2v13\"/><circle cx=\"6\" cy=\"18\" r=\"3\"/><circle cx=\"16\" cy=\"16\" r=\"3\"/></svg>"
                 + "<h3>" + E(title) + "</h3><p>" + E(message) + "</p></div>";
        }

        public static string Alert(string message, bool isError)
        {
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }
            return "<div class=\"alert " + (isError ? "alert-error" : "alert-success") + "\" role=\"status\">" +
                   E(message) + "</div>";
        }

        public static string StatCard(string value, string label, string hint)
        {
            return "<div class=\"pm-stat\"><div class=\"pm-stat-value\">" + E(value) + "</div>" +
                   "<div class=\"pm-stat-label\">" + E(label) + "</div>" +
                   (string.IsNullOrEmpty(hint) ? string.Empty : "<div class=\"pm-stat-hint\">" + E(hint) + "</div>") +
                   "</div>";
        }

        public static string SectionHead(string title, string moreUrl, string moreText)
        {
            string more = string.IsNullOrEmpty(moreUrl)
                ? string.Empty
                : "<a class=\"pm-more\" href=\"" + A(moreUrl) + "\">" + E(moreText) + " &rarr;</a>";
            return "<div class=\"pm-section-head\"><h2>" + E(title) + "</h2>" + more + "</div>";
        }

        public static string PageHead(string title, string subtitle, string actionsHtml)
        {
            string sub = string.IsNullOrEmpty(subtitle)
                ? string.Empty
                : "<p class=\"pm-page-sub\">" + E(subtitle) + "</p>";
            return "<div class=\"pm-page-head\"><div><h1>" + E(title) + "</h1>" + sub + "</div>" +
                   (string.IsNullOrEmpty(actionsHtml) ? string.Empty : "<div class=\"pm-page-actions\">" + actionsHtml + "</div>") +
                   "</div>";
        }

        /// <summary>Campo oculto con el token anti-CSRF.</summary>
        public static string CsrfField(string token)
        {
            return "<input type=\"hidden\" name=\"__PoliMusicToken\" value=\"" + A(token) + "\" />";
        }

        /// <summary>Boton de envio con estilo uniforme.</summary>
        public static string Submit(string text, string cssClass)
        {
            string css = string.IsNullOrEmpty(cssClass) ? "pm-btn" : cssClass;
            return "<button type=\"submit\" class=\"" + A(css) + "\">" + E(text) + "</button>";
        }
    }
}
