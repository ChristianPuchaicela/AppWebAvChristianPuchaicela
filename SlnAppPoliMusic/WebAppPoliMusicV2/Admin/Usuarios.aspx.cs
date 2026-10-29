using System;
using System.Collections.Generic;
using System.Text;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Gestion de usuarios (solo administradores): cambio de rol y borrado.
    /// Las reglas de negocio (ultimo administrador, cuenta propia, playlists
    /// asociadas) viven en UserBll.
    /// </summary>
    public partial class AdminUsuarios : PageBase
    {
        protected string RowsHtml;
        protected string ErrorMessage;

        private readonly UserBll _users = new UserBll();
        private readonly PlaylistBll _playlists = new PlaylistBll();

        public AdminUsuarios()
        {
            RequiresAdmin = true;
            ActiveSection = "admin-users";
            RowsHtml = string.Empty;
            ErrorMessage = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                HandlePost();
                return;
            }
            LoadRows();
        }

        private void HandlePost()
        {
            if (!ValidateCsrf())
            {
                ErrorMessage = "La sesion del formulario expiro. Recargue la pagina.";
                LoadRows();
                return;
            }

            int deleteId = FormInt("deleteId");
            if (deleteId > 0)
            {
                OperationResult deleted = _users.Delete(deleteId, CurrentUserId);
                RedirectWithFlash("~/Admin/Usuarios.aspx", deleted.Message, !deleted.Success);
                return;
            }

            int toggleId = FormInt("toggleRole");
            if (toggleId > 0)
            {
                ToggleRole(toggleId);
                return;
            }

            LoadRows();
        }

        private void ToggleRole(int idUser)
        {
            User user = _users.ReadById(idUser);
            if (user == null)
            {
                RedirectWithFlash("~/Admin/Usuarios.aspx", "El usuario ya no existe.", true);
                return;
            }

            user.UserType = user.IsAdmin ? Constants.ID_USER_NORMAL : Constants.ID_USER_ADMIN;
            OperationResult result = _users.Update(user);
            RedirectWithFlash("~/Admin/Usuarios.aspx", result.Message, !result.Success);
        }

        private void LoadRows()
        {
            List<User> users = _users.Read();
            StringBuilder sb = new StringBuilder();

            foreach (User user in users)
            {
                string badge = user.IsAdmin
                    ? "<span class=\"pm-badge pm-badge-admin\">Admin</span>"
                    : "<span class=\"pm-badge pm-badge-user\">Usuario</span>";

                string roleAction = user.IsAdmin
                    ? "<button type=\"submit\" class=\"pm-btn pm-btn-ghost pm-btn-sm\" name=\"toggleRole\" value=\"" +
                      user.IdUser + "\" data-pm-confirm=\"\u00BFQuitar el rol de administrador a " +
                      Ui.A(user.Username) + "?\">Quitar admin</button>"
                    : "<button type=\"submit\" class=\"pm-btn pm-btn-ghost pm-btn-sm\" name=\"toggleRole\" value=\"" +
                      user.IdUser + "\" data-pm-confirm=\"\u00BFPromover a " + Ui.A(user.Username) +
                      " a administrador?\">Hacer admin</button>";

                string deleteAction = user.IdUser == CurrentUserId
                    ? "<span class=\"pm-badge pm-badge-muted\">Tu cuenta</span>"
                    : "<button type=\"submit\" class=\"pm-btn pm-btn-danger pm-btn-sm\" name=\"deleteId\" value=\"" +
                      user.IdUser + "\" data-pm-confirm=\"\u00BFEliminar la cuenta " + Ui.A(user.Username) +
                      "? Esta accion no se puede deshacer.\">Eliminar</button>";

                sb.Append("<tr>");
                sb.Append("<td data-label=\"Usuario\"><strong>").Append(Ui.E(user.Username)).Append("</strong></td>");
                sb.Append("<td data-label=\"Correo\">").Append(Ui.E(user.Email)).Append("</td>");
                sb.Append("<td data-label=\"Rol\">").Append(badge).Append("</td>");
                sb.Append("<td data-label=\"Nacimiento\">")
                  .Append(user.Birthday.HasValue ? user.Birthday.Value.ToString("dd/MM/yyyy") : "&mdash;")
                  .Append("</td>");
                sb.Append("<td data-label=\"Playlists\" class=\"pm-plays\">")
                  .Append(_playlists.ReadOwnedBy(user.IdUser).Count).Append("</td>");
                sb.Append("<td data-label=\"Acciones\"><div class=\"pm-row-actions\">")
                  .Append(roleAction).Append(deleteAction).Append("</div></td>");
                sb.Append("</tr>");
            }
            RowsHtml = sb.ToString();
        }
    }
}
