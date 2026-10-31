using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebAppPoliMusicV3
{
    public partial class Song : System.Web.UI.Page
    {
        private static string ConnectionString
        {
            get
            {
                var conn = ConfigurationManager.ConnectionStrings["BDD_PoliMusicConnectionString"];
                return conn != null ? conn.ConnectionString : string.Empty;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSongs();
            }
        }

        private void LoadSongs(string searchTerm = null)
        {
            try
            {
                string connStr = ConnectionString;
                if (string.IsNullOrEmpty(connStr))
                {
                    ShowMessage("No se ha configurado la cadena de conexión BDD_PoliMusicConnectionString.", true);
                    return;
                }

                // Llamada a la capa de negocio (BLL)
                BLL.PoliMusicV3.Song songBll = new BLL.PoliMusicV3.Song(connStr);
                DataTable dtSongs = songBll.Read();

                if (dtSongs == null)
                {
                    ShowMessage("No se pudieron cargar las canciones desde la base de datos.", true);
                    return;
                }

                // Cálculo de estadísticas generales
                int totalSongs = dtSongs.Rows.Count;
                long totalPlays = 0;
                foreach (DataRow row in dtSongs.Rows)
                {
                    if (row["PLAYS"] != DBNull.Value)
                    {
                        totalPlays += Convert.ToInt64(row["PLAYS"]);
                    }
                }

                lblTotalSongs.Text = totalSongs.ToString("N0");
                lblTotalPlays.Text = totalPlays.ToString("N0");

                // Filtro opcional por búsqueda
                DataView dv = dtSongs.DefaultView;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    string safeTerm = searchTerm.Trim().Replace("'", "''");
                    dv.RowFilter = string.Format("SONG_NAME LIKE '%{0}%' OR ARTIST_LABEL LIKE '%{0}%'", safeTerm);
                }

                gvSongs.DataSource = dv;
                gvSongs.DataBind();

                if (dv.Count == 0 && !string.IsNullOrWhiteSpace(searchTerm))
                {
                    ShowMessage(string.Format("No se encontraron canciones que coincidan con \"{0}\".", searchTerm), false);
                }
                else
                {
                    lblMessage.Visible = false;
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error al cargar canciones: " + ex.Message, true);
            }
        }

        protected void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadSongs(txtSearch.Text);
        }

        protected void BtnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            LoadSongs();
        }

        protected void GvSongs_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Personalización adicional de las filas si fuese necesario
        }

        /// <summary>
        /// Resuelve la URL web para reproducir el archivo de audio MP3
        /// según la convención de carpetas de PoliMusic.
        /// </summary>
        public string ResolveSongUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            string clean = path.Trim().Replace('\\', '/');

            // Caso 1: Ruta virtual con ~
            if (clean.StartsWith("~/"))
            {
                return ResolveUrl(clean);
            }

            // Caso 2: Ruta relativa "../songFiles/..."
            if (clean.StartsWith("../songFiles/", StringComparison.OrdinalIgnoreCase))
            {
                return ResolveUrl("~/SongFiles/" + clean.Substring("../songFiles/".Length));
            }
            if (clean.StartsWith("../"))
            {
                return ResolveUrl("~/" + clean.Substring(3));
            }

            // Caso 3: Ruta que ya empieza con /SongFiles o SongFiles/
            if (clean.StartsWith("songFiles/", StringComparison.OrdinalIgnoreCase))
            {
                return ResolveUrl("~/" + clean);
            }

            if (clean.StartsWith("/"))
            {
                return clean;
            }

            // Caso 4: Solo nombre del archivo
            string fileName = Path.GetFileName(clean);
            return ResolveUrl("~/SongFiles/" + fileName);
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "alert alert-danger d-block mb-3" : "alert alert-info d-block mb-3";
            lblMessage.Visible = true;
        }

        /// <summary>
        /// Endpoint AJAX para registrar e incrementar una reproducción atómicamente en SQL Server.
        /// </summary>
        [WebMethod]
        public static bool IncrementPlay(int idSong)
        {
            if (idSong <= 0) return false;
            try
            {
                string connStr = ConnectionString;
                if (string.IsNullOrEmpty(connStr)) return false;

                BLL.PoliMusicV3.Song songBll = new BLL.PoliMusicV3.Song(connStr);
                int rows = songBll.IncrementPlays(idSong);
                return rows > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
