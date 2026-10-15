using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Genera el HTML de las tarjetas y filas que comparten las paginas del
    /// catalogo. Centraliza el escapado y la resolucion de rutas multimedia.
    /// </summary>
    public static class Ui
    {
        private const string IconPlay =
            "<svg viewBox=\"0 0 24 24\" fill=\"currentColor\"><path d=\"M8 5v14l11-7z\"/></svg>";

        private static string E(string v)
        {
            return HttpUtility.HtmlEncode(v ?? string.Empty);
        }

        private static string A(string v)
        {
            return HttpUtility.HtmlAttributeEncode(v ?? string.Empty);
        }

        /// <summary>Iniciales de un nombre, para el marcador de posicion de portada.</summary>
        public static string InitialsOf(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "♪";
            }
            string t = text.Trim();
            string[] parts = t.Split(new[] { ' ', '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpperInvariant();
            }
            return t.Substring(0, Math.Min(2, t.Length)).ToUpperInvariant();
        }

        /// <summary>
        /// Serializa una cancion para el reproductor en un atributo de datos.
        /// El titulo y el artista se escapan para uso seguro dentro del HTML.
        /// </summary>
        public static string PlayPayload(Song song, string artistLabel, string artUrl)
        {
            string src = MediaResolver.Resolve(song != null ? song.SongPath : null);
            if (src == null)
            {
                return null;
            }
            var sb = new StringBuilder();
            sb.Append("{\"id\":").Append(song.IdSong);
            sb.Append(",\"src\":\"").Append(A(src)).Append('"');
            sb.Append(",\"title\":\"").Append(A(song.SongName)).Append('"');
            sb.Append(",\"artist\":\"").Append(A(artistLabel)).Append('"');
            sb.Append(",\"art\":\"").Append(A(artUrl)).Append('"');
            sb.Append(",\"initials\":\"").Append(A(InitialsOf(song.SongName))).Append('"');
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>Cola de reproduccion completa serializada para navegacion.</summary>
        public static string PlayQueue(IEnumerable<Song> songs, Func<Song, string> artistOf, string artUrl)
        {
            var sb = new StringBuilder("[");
            bool first = true;
            foreach (Song s in songs)
            {
                string payload = PlayPayload(s, artistOf(s), artUrl);
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

        /// <summary>Tarjeta de cancion con boton de reproduccion.</summary>
        public static string SongCard(Song song, string artistLabel, string artUrl, string queueJson)
        {
            string play = PlayPayload(song, artistLabel, artUrl);
            string art = artUrl != null
                ? "<img src=\"" + A(artUrl) + "\" alt=\"\" loading=\"lazy\" onerror=\"this.style.display='none';this.parentNode.classList.add('noimg')\">"
                : "<span class=\"pm-art-initials\">" + E(InitialsOf(song.SongName)) + "</span>";

            var sb = new StringBuilder();
            sb.Append("<article class=\"pm-card\">");
            sb.Append("<div class=\"pm-card-art\">").Append(art);
            if (play != null)
            {
                sb.Append("<button type=\"button\" class=\"pm-card-play\" data-pm-play=\"")
                  .Append(A(play)).Append("\" data-pm-queue=\"").Append(A(queueJson))
                  .Append("\" aria-label=\"Reproducir ").Append(A(song.SongName)).Append("\">")
                  .Append(IconPlay).Append("</button>");
            }
            sb.Append("</div>");
            sb.Append("<div class=\"pm-card-title\">").Append(E(song.SongName)).Append("</div>");
            sb.Append("<div class=\"pm-card-sub\">").Append(E(artistLabel ?? "Sin artista")).Append("</div>");
            sb.Append("</article>");
            return sb.ToString();
        }

        /// <summary>Tarjeta de album, enlazada a su detalle.</summary>
        public static string AlbumCard(Album album, string artUrl, string detailUrl)
        {
            string art = artUrl != null
                ? "<img src=\"" + A(artUrl) + "\" alt=\"\" loading=\"lazy\" onerror=\"this.style.display='none'\">"
                : "<span class=\"pm-art-initials\">" + E(InitialsOf(album.TitleAlbum)) + "</span>";

            return "<article class=\"pm-card\">"
                 + "<a class=\"pm-card-link\" href=\"" + A(detailUrl) + "\" aria-label=\"" + A(album.TitleAlbum) + "\"></a>"
                 + "<div class=\"pm-card-art\">" + art + "</div>"
                 + "<div class=\"pm-card-title\">" + E(album.TitleAlbum) + "</div>"
                 + "<div class=\"pm-card-sub\">" + (album.ReleaseYear.HasValue ? album.ReleaseYear.Value.ToString() : "Año desconocido")
                 + " · " + album.SongCount + " canción(es)</div>"
                 + "</article>";
        }

        /// <summary>Tarjeta de artista, enlazada a su detalle.</summary>
        public static string ArtistCard(Artist artist, string artUrl, string detailUrl)
        {
            string art = artUrl != null
                ? "<img src=\"" + A(artUrl) + "\" alt=\"\" loading=\"lazy\" onerror=\"this.style.display='none'\">"
                : "<span class=\"pm-art-initials\">" + E(InitialsOf(artist.FullName)) + "</span>";

            return "<article class=\"pm-card\" style=\"text-align:center;\">"
                 + "<a class=\"pm-card-link\" href=\"" + A(detailUrl) + "\" aria-label=\"" + A(artist.FullName) + "\"></a>"
                 + "<div class=\"pm-card-art round\">" + art + "</div>"
                 + "<div class=\"pm-card-title\">" + E(artist.FullName) + "</div>"
                 + "<div class=\"pm-card-sub\">Artista</div>"
                 + "</article>";
        }

        /// <summary>Tarjeta de playlist con etiqueta de visibilidad.</summary>
        public static string PlaylistCard(Playlist playlist, string artUrl, string detailUrl)
        {
            string art = artUrl != null
                ? "<img src=\"" + A(artUrl) + "\" alt=\"\" loading=\"lazy\" onerror=\"this.style.display='none'\">"
                : "<span class=\"pm-art-initials\">" + E(InitialsOf(playlist.Title)) + "</span>";

            string badge = playlist.IsPrivate
                ? "<span class=\"pm-badge pm-badge-private\">Privada</span>"
                : "<span class=\"pm-badge pm-badge-public\">Pública</span>";

            return "<article class=\"pm-card\">"
                 + "<a class=\"pm-card-link\" href=\"" + A(detailUrl) + "\" aria-label=\"" + A(playlist.Title) + "\"></a>"
                 + "<div class=\"pm-card-art\">" + art + "</div>"
                 + "<div class=\"pm-card-title\">" + E(playlist.Title) + "</div>"
                 + "<div class=\"pm-card-sub\">" + playlist.SongCount + " canción(es)</div>"
                 + "<div style=\"margin-top:8px;\">" + badge + "</div>"
                 + "</article>";
        }

        /// <summary>Estado vacio reutilizable.</summary>
        public static string EmptyState(string title, string message)
        {
            return "<div class=\"pm-empty\">"
                 + "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\" stroke-linecap=\"round\">"
                 + "<path d=\"M9 18V5l10-2v13\"/><circle cx=\"6\" cy=\"18\" r=\"3\"/><circle cx=\"16\" cy=\"16\" r=\"3\"/></svg>"
                 + "<h3>" + E(title) + "</h3><p>" + E(message) + "</p></div>";
        }

        /// <summary>
        /// Fila de tabla de cancion con boton de reproduccion individual.
        /// La cola se comparte entre todas las filas de la pagina.
        /// </summary>
        public static string SongRow(Song song, string artistLabel, string artUrl, string queueJson,
                                     int index, string extraActionsHtml = null)
        {
            return SongRow(song, artistLabel, artUrl, queueJson, index, extraActionsHtml, null);
        }

        /// <summary>
        /// Fila de tabla de cancion. <paramref name="albumLabel"/> es opcional:
        /// solo el catalogo de canciones muestra la columna de album.
        /// </summary>
        public static string SongRow(Song song, string artistLabel, string artUrl, string queueJson,
                                     int index, string extraActionsHtml, string albumLabel)
        {
            string play = PlayPayload(song, artistLabel, artUrl);
            var sb = new StringBuilder();
            sb.Append("<tr>");
            sb.Append("<td data-label=\"#\"><div class=\"pm-song-row\"><span class=\"pm-song-index\">")
              .Append(index).Append("</span>");

            if (play != null)
            {
                sb.Append("<button type=\"button\" class=\"pm-play-mini\" data-pm-play=\"")
                  .Append(A(play)).Append("\" data-pm-queue=\"").Append(A(queueJson))
                  .Append("\" aria-label=\"Reproducir ").Append(A(song.SongName)).Append("\">")
                  .Append(IconPlay).Append("</button>");
            }
            else
            {
                sb.Append("<span class=\"pm-play-mini\" style=\"opacity:0.35;cursor:not-allowed;\" title=\"Sin archivo de audio\">")
                  .Append(IconPlay).Append("</span>");
            }

            sb.Append("</div></td>");
            sb.Append("<td data-label=\"Canción\"><strong>").Append(E(song.SongName)).Append("</strong></td>");
            if (albumLabel != null)
            {
                sb.Append("<td data-label=\"Álbum\">").Append(E(albumLabel)).Append("</td>");
            }
            sb.Append("<td data-label=\"Artista\">").Append(E(artistLabel ?? "Sin artista")).Append("</td>");
            sb.Append("<td data-label=\"Reproducciones\" class=\"pm-plays\">").Append(song.Plays ?? 0).Append("</td>");
            sb.Append("<td data-label=\"Acciones\"><div class=\"pm-row-actions\">").Append(extraActionsHtml ?? string.Empty).Append("</div></td>");
            sb.Append("</tr>");
            return sb.ToString();
        }
    }
}
