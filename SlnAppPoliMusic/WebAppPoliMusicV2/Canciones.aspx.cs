using System;
using System.Collections.Generic;
using System.Text;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Catalogo de canciones con busqueda por nombre. Cada fila incluye el
    /// boton de reproduccion y, para administradores, el acceso a la edicion.
    /// </summary>
    public partial class Canciones : PageBase
    {
        protected string Subtitle;
        protected string SearchTerm;
        protected string RowsHtml;

        public Canciones()
        {
            ActiveSection = "songs";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SearchTerm = QueryStr("q");

            List<Song> songs = new SongBll().Read(SearchTerm);
            Subtitle = songs.Count == 1
                ? "1 cancion en el catalogo"
                : songs.Count + " canciones en el catalogo";

            if (SearchTerm.Length > 0)
            {
                Subtitle = songs.Count + " resultado(s) para \"" + SearchTerm + "\"";
            }

            string queue = Ui.PlayQueue(songs, null);
            StringBuilder sb = new StringBuilder();
            int index = 1;
            foreach (Song song in songs)
            {
                string actions = string.Empty;
                if (IsAdmin)
                {
                    actions = "<a class=\"pm-btn pm-btn-ghost pm-btn-sm\" href=\"" +
                              ResolveUrl("~/Admin/Canciones.aspx?edit=" + song.IdSong) + "\">Editar</a>";
                }
                sb.Append(Ui.SongRow(song, index, queue, actions));
                index++;
            }
            RowsHtml = sb.ToString();
        }
    }
}
