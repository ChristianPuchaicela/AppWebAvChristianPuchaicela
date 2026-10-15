using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebAppPoliMusicV1.Data;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1.Admin
{
    /// <summary>
    /// CRUD de usuarios (solo administradores).
    ///
    /// Reglas de integridad aplicadas:
    ///  - Un administrador no puede eliminarse a si mismo.
    ///  - No se puede eliminar al ultimo administrador del sistema.
    ///  - Antes de eliminar se informa cuantas playlists dependen del usuario
    ///    (FK_TBL_PLAYLIST_TBL_USER); la eliminacion arrastra esas playlists en
    ///    una transaccion, respetando TBL_PLAYLIST_SONG.
    /// </summary>
    public partial class Usuarios : BasePage
    {
        public Usuarios()
        {
            // Solo administradores: la comprobacion ocurre en el servidor.
            RequiresAdmin = true;
        }

        protected List<User> Users = new List<User>();
        protected List<string> Rows = new List<string>();
        protected User EditUser { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserContext.IsAuthenticated)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }
            if (!UserContext.IsAdmin)
            {
                Response.StatusCode = 403;
                Response.Redirect("~/AccesoDenegado.aspx", true);
                return;
            }

            ((SiteMaster)Master).ActiveSection = "admin-users";

            if (!IsPostBack)
            {
                BindRoles();

                string action = Request.QueryString["action"];
                int id = QueryInt("id");

                if (string.Equals(action, "delete", StringComparison.OrdinalIgnoreCase) && id > 0)
                {
                    DeleteUser(id);
                }
                else if (id > 0)
                {
                    EditUser = UserRepository.GetById(id);
                    if (EditUser != null)
                    {
                        BindEditFields(EditUser);
                    }
                }
                else if (string.Equals(Request.QueryString["deleted"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("El usuario se eliminó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["created"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("El usuario se creó correctamente.", false);
                }
                else if (string.Equals(Request.QueryString["updated"], "1", StringComparison.Ordinal))
                {
                    ShowMessage("El usuario se actualizó correctamente.", false);
                }
            }

            LoadUsers();
        }

        private void LoadUsers()
        {
            Users = UserRepository.GetAll();

            foreach (User u in Users)
            {
                Rows.Add(BuildRow(u));
            }
        }

        private string BuildRow(User u)
        {
            int playlists = UserRepository.CountPlaylists(u.IdUser);
            bool isSelf = u.IdUser == UserContext.CurrentUserId;

            var sb = new StringBuilder();
            sb.Append("<tr>");
            sb.Append("<td data-label=\"ID\" class=\"pm-plays\">").Append(u.IdUser).Append("</td>");
            sb.Append("<td data-label=\"Usuario\"><strong>").Append(H(u.Username)).Append("</strong>");
            if (isSelf)
            {
                sb.Append(" <span class=\"pm-badge pm-badge-muted\">usted</span>");
            }
            sb.Append("</td>");
            sb.Append("<td data-label=\"Correo\">").Append(H(u.Email)).Append("</td>");
            sb.Append("<td data-label=\"Rol\">");
            sb.Append(u.IsAdmin
                ? "<span class=\"pm-badge pm-badge-admin\">Administrador</span>"
                : "<span class=\"pm-badge pm-badge-user\">Usuario</span>");
            sb.Append("</td>");
            sb.Append("<td data-label=\"Nacimiento\">")
              .Append(u.Birthday.HasValue ? u.Birthday.Value.ToString("dd/MM/yyyy") : "—")
              .Append("</td>");
            sb.Append("<td data-label=\"Playlists\" class=\"pm-plays\">").Append(playlists).Append("</td>");

            sb.Append("<td data-label=\"Acciones\"><div class=\"pm-row-actions\">");
            sb.Append("<a class=\"pm-btn pm-btn-sm pm-btn-ghost\" href=\"")
              .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Admin/Usuarios.aspx?id=" + u.IdUser)))
              .Append("\">Editar</a>");

            if (isSelf)
            {
                // No se permite que un administrador elimine su propia cuenta.
                sb.Append("<span class=\"pm-btn pm-btn-sm\" style=\"opacity:0.4;cursor:not-allowed;\" title=\"No puede eliminar su propia cuenta\">Eliminar</span>");
            }
            else
            {
                string confirm = playlists > 0
                    ? "¿Eliminar el usuario «" + u.Username + "»? Se eliminarán también sus "
                      + playlists + " playlist(s) asociadas. Esta acción no se puede deshacer."
                    : "¿Eliminar el usuario «" + u.Username + "»? Esta acción no se puede deshacer.";

                sb.Append("<a class=\"pm-btn pm-btn-sm pm-btn-danger\" href=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(ResolveUrl("~/Admin/Usuarios.aspx?action=delete&id=" + u.IdUser
                      + "&token=" + GetCsrf())))
                  .Append("\" data-pm-confirm=\"")
                  .Append(HttpUtility.HtmlAttributeEncode(confirm))
                  .Append("\">Eliminar</a>");
            }
            sb.Append("</div></td>");
            sb.Append("</tr>");
            return sb.ToString();
        }

        private void DeleteUser(int id)
        {
            if (!IsCsrfValid())
            {
                ShowMessage("Solicitud no válida. Vuelva a intentarlo.", true);
                return;
            }

            // Un administrador no puede eliminarse a si mismo.
            if (id == UserContext.CurrentUserId)
            {
                ShowMessage("No puede eliminar su propia cuenta mientras la usa.", true);
                return;
            }

            User target = UserRepository.GetById(id);
            if (target == null)
            {
                ShowMessage("El usuario indicado no existe.", true);
                return;
            }

            // No se permite dejar el sistema sin administradores.
            if (target.IsAdmin && UserRepository.CountAdmins() <= 1)
            {
                ShowMessage("No se puede eliminar al último administrador del sistema.", true);
                return;
            }

            try
            {
                DeleteUserCascade(id);
                Response.Redirect("~/Admin/Usuarios.aspx?deleted=1", true);
            }
            catch (Exception ex)
            {
                ShowMessage("No se pudo eliminar el usuario: " + ex.Message, true);
            }
        }

        /// <summary>
        /// Elimina el usuario y sus dependencias en una transaccion, respetando
        /// el orden que exigen las claves foraneas.
        /// </summary>
        private static void DeleteUserCascade(int userId)
        {
            using (System.Data.SqlClient.SqlConnection cn = Db.OpenConnection())
            using (System.Data.SqlClient.SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    // 1) Filas de TBL_PLAYLIST_SONG de las playlists del usuario.
                    using (var c = Db.Command(cn,
                        "DELETE ps FROM TBL_PLAYLIST_SONG ps " +
                        "INNER JOIN TBL_PLAYLIST p ON p.ID_PLAYLIST = ps.ID_PLAYLIST " +
                        "WHERE p.ID_USER = @u", Db.P("@u", userId)))
                    { c.Transaction = tx; c.ExecuteNonQuery(); }

                    // 2) Playlists del usuario.
                    using (var c = Db.Command(cn, "DELETE FROM TBL_PLAYLIST WHERE ID_USER = @u", Db.P("@u", userId)))
                    { c.Transaction = tx; c.ExecuteNonQuery(); }

                    // 3) El usuario.
                    using (var c = Db.Command(cn, "DELETE FROM TBL_USER WHERE ID_USER = @u", Db.P("@u", userId)))
                    { c.Transaction = tx; c.ExecuteNonQuery(); }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string username = (txtNewUsername.Text ?? string.Empty).Trim();
            string email = (txtNewEmail.Text ?? string.Empty).Trim();
            string password = txtNewPassword.Text ?? string.Empty;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowMessage("Usuario, correo y contraseña son obligatorios.", true);
                return;
            }
            if (username.Length < 4 || username.Length > 50)
            {
                ShowMessage("El usuario debe tener entre 4 y 50 caracteres.", true);
                return;
            }
            if (password.Length < 8)
            {
                ShowMessage("La contraseña debe tener al menos 8 caracteres.", true);
                return;
            }
            if (UserRepository.UsernameExists(username, 0))
            {
                ShowMessage("Ese nombre de usuario ya está en uso.", true);
                return;
            }
            if (UserRepository.EmailExists(email, 0))
            {
                ShowMessage("Ese correo ya está registrado.", true);
                return;
            }

            int userType = 0;
            int.TryParse(ddlNewRole.SelectedValue, out userType);

            DateTime birthday;
            DateTime? parsed = null;
            if (!string.IsNullOrEmpty(txtNewBirthday.Text) && DateTime.TryParse(txtNewBirthday.Text, out birthday))
            {
                parsed = birthday;
            }

            UserRepository.Create(username, PasswordHasher.Hash(password), email, parsed,
                "~/userImages/default.jpg", userType);

            Response.Redirect("~/Admin/Usuarios.aspx?created=1", true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = QueryInt("id");
            User target = UserRepository.GetById(id);
            if (target == null)
            {
                ShowMessage("El usuario indicado no existe.", true);
                return;
            }

            string username = (txtUsername.Text ?? string.Empty).Trim();
            string email = (txtEmail.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                ShowMessage("El usuario y el correo son obligatorios.", true);
                return;
            }
            if (UserRepository.UsernameExists(username, id))
            {
                ShowMessage("Ese nombre de usuario ya está en uso.", true);
                return;
            }
            if (UserRepository.EmailExists(email, id))
            {
                ShowMessage("Ese correo ya está registrado.", true);
                return;
            }

            int userType = 0;
            int.TryParse(ddlRole.SelectedValue, out userType);

            // No se permite degradar al ultimo administrador del sistema.
            if (target.IsAdmin && userType != UserRoles.Admin && UserRepository.CountAdmins() <= 1)
            {
                ShowMessage("No se puede quitar el rol al último administrador del sistema.", true);
                return;
            }

            DateTime birthday;
            DateTime? parsed = null;
            if (!string.IsNullOrEmpty(txtBirthday.Text) && DateTime.TryParse(txtBirthday.Text, out birthday))
            {
                parsed = birthday;
            }

            string photo = string.IsNullOrEmpty(ddlPhoto.SelectedValue) ? target.UserPhoto : ddlPhoto.SelectedValue;

            UserRepository.Update(id, username, email, parsed, photo, userType);

            // Restablecimiento de contraseña opcional por parte del administrador.
            if (!string.IsNullOrEmpty(txtResetPassword.Text))
            {
                if (txtResetPassword.Text.Length < 8)
                {
                    ShowMessage("El usuario se actualizó, pero la nueva contraseña es demasiado corta (mínimo 8).", true);
                    return;
                }
                UserRepository.UpdatePassword(id, PasswordHasher.Hash(txtResetPassword.Text));
            }

            Response.Redirect("~/Admin/Usuarios.aspx?updated=1", true);
        }

        private void BindRoles()
        {
            ddlNewRole.Items.Clear();
            ddlRole.Items.Clear();

            // Los roles provienen de TBL_CATALOG (PARENT = '1').
            List<CatalogItem> roles = CatalogRepository.GetByParent("1");
            if (roles.Count == 0)
            {
                roles.Add(new CatalogItem { CodCatalog = "0", Value = "Usuario" });
                roles.Add(new CatalogItem { CodCatalog = "1", Value = "Administrador" });
            }

            foreach (CatalogItem item in roles)
            {
                var newItem = new ListItem(item.Value, item.CodCatalog);
                ddlNewRole.Items.Add(new ListItem(item.Value, item.CodCatalog));
                ddlRole.Items.Add(newItem);
            }
        }

        private void BindEditFields(User u)
        {
            txtUsername.Text = u.Username;
            txtEmail.Text = u.Email;
            txtBirthday.Text = u.Birthday.HasValue ? u.Birthday.Value.ToString("yyyy-MM-dd") : string.Empty;

            ListItem role = ddlRole.Items.FindByValue(u.UserType.ToString());
            if (role != null)
            {
                ddlRole.ClearSelection();
                role.Selected = true;
            }

            ddlPhoto.Items.Clear();
            string physical = Server.MapPath("~/Assets/UserImages");
            if (System.IO.Directory.Exists(physical))
            {
                var files = System.IO.Directory.GetFiles(physical);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                foreach (string f in files)
                {
                    ddlPhoto.Items.Add(new ListItem(
                        System.IO.Path.GetFileName(f), "~/userImages/" + System.IO.Path.GetFileName(f)));
                }
            }
            if (ddlPhoto.Items.Count == 0)
            {
                ddlPhoto.Items.Add(new ListItem("Predeterminada", "~/userImages/default.jpg"));
            }

            ListItem current = ddlPhoto.Items.FindByValue(u.UserPhoto);
            if (current != null)
            {
                ddlPhoto.ClearSelection();
                current.Selected = true;
            }
        }

        private bool IsCsrfValid()
        {
            string expected = Session["PM_CSRF"] as string;
            string received = Request.QueryString["token"] ?? Request.Form["__PoliMusicToken"];
            return !string.IsNullOrEmpty(expected) && !string.IsNullOrEmpty(received)
                && string.Equals(expected, received, StringComparison.Ordinal);
        }

        private string GetCsrf()
        {
            const string key = "PM_CSRF";
            string token = Session[key] as string;
            if (string.IsNullOrEmpty(token))
            {
                token = Guid.NewGuid().ToString("N");
                Session[key] = token;
            }
            return token;
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "alert " + (isError ? "alert-error" : "alert-success");
        }

        protected static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
