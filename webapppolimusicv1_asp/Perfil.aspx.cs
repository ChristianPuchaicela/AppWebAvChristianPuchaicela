using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebAppPoliMusicV1.Data;
using WebAppPoliMusicV1.Models;
using WebAppPoliMusicV1.Services;

namespace WebAppPoliMusicV1
{
    /// <summary>
    /// Perfil del usuario autenticado. Permite editar sus propios datos y su
    /// contrasena, pero nunca su rol: USER_TYPE solo lo cambia un administrador
    /// desde el panel de administracion.
    /// </summary>
    public partial class Perfil : BasePage
    {
        protected User User { get; set; }
        protected string PhotoUrl { get; set; }
        protected string RoleName { get; set; }
        protected string Initials { get; set; }
        protected int PlaylistCount { get; set; }

        /// <summary>Solo se ofrece el selector si hay imagenes de usuario en disco.</summary>
        protected bool CanChoosePhoto { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserContext.IsAuthenticated)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            ((SiteMaster)Master).ActiveSection = "profile";

            User = UserRepository.GetById(UserContext.CurrentUserId);
            if (User == null)
            {
                // La cuenta fue eliminada mientras la sesion seguia activa.
                UserContext.SignOut();
                Response.Redirect("~/Login.aspx?msg=expired", true);
                return;
            }

            PhotoUrl = MediaResolver.Resolve(User.UserPhoto);
            RoleName = UserRoles.Name(User.UserType);
            Initials = Ui.InitialsOf(User.Username);
            PlaylistCount = UserRepository.CountPlaylists(User.IdUser);

            if (!IsPostBack)
            {
                txtUsername.Text = User.Username;
                txtEmail.Text = User.Email;
                txtBirthday.Text = User.Birthday.HasValue
                    ? User.Birthday.Value.ToString("yyyy-MM-dd")
                    : string.Empty;

                BindPhotoOptions();
            }
        }

        private void BindPhotoOptions()
        {
            ddlPhoto.Items.Clear();
            string physical = Server.MapPath("~/Assets/UserImages");
            if (System.IO.Directory.Exists(physical))
            {
                var files = System.IO.Directory.GetFiles(physical);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                foreach (string f in files)
                {
                    string name = System.IO.Path.GetFileName(f);
                    ddlPhoto.Items.Add(new ListItem(name, "~/userImages/" + name));
                }
            }

            CanChoosePhoto = ddlPhoto.Items.Count > 0;

            if (CanChoosePhoto)
            {
                ListItem current = ddlPhoto.Items.FindByValue(User.UserPhoto);
                if (current != null)
                {
                    ddlPhoto.ClearSelection();
                    current.Selected = true;
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string username = (txtUsername.Text ?? string.Empty).Trim();
            string email = (txtEmail.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                ShowMessage("El usuario y el correo son obligatorios.", true);
                return;
            }
            if (username.Length < 4 || username.Length > 50)
            {
                ShowMessage("El usuario debe tener entre 4 y 50 caracteres.", true);
                return;
            }
            if (UserRepository.UsernameExists(username, User.IdUser))
            {
                ShowMessage("Ese nombre de usuario ya está en uso.", true);
                return;
            }
            if (UserRepository.EmailExists(email, User.IdUser))
            {
                ShowMessage("Ese correo ya está registrado por otra cuenta.", true);
                return;
            }

            DateTime birthday;
            DateTime? parsedBirthday = null;
            if (!string.IsNullOrEmpty(txtBirthday.Text) && DateTime.TryParse(txtBirthday.Text, out birthday))
            {
                parsedBirthday = birthday;
            }

            string photo = User.UserPhoto;
            if (CanChoosePhoto && !string.IsNullOrEmpty(ddlPhoto.SelectedValue))
            {
                photo = ddlPhoto.SelectedValue;
            }

            // El rol se conserva tal como esta en la base de datos: el formulario
            // no puede elevarlo. El parametro UserType viene del registro actual.
            UserRepository.Update(User.IdUser, username, email, parsedBirthday, photo, User.UserType);

            User updated = UserRepository.GetById(User.IdUser);
            UserContext.Refresh(updated);

            User = updated;
            PhotoUrl = MediaResolver.Resolve(updated.UserPhoto);
            Initials = Ui.InitialsOf(updated.Username);

            ShowMessage("Sus datos se actualizaron correctamente.", false);
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            AuthResult result = AuthService.ChangePassword(
                User.IdUser, txtCurrent.Text, txtNew.Text, txtConfirm.Text);

            if (!result.Success)
            {
                ShowMessage(result.Error, true);
                return;
            }

            txtCurrent.Text = string.Empty;
            txtNew.Text = string.Empty;
            txtConfirm.Text = string.Empty;
            ShowMessage("Su contraseña se actualizó correctamente.", false);
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

        protected static string Url(string value)
        {
            return HttpUtility.HtmlAttributeEncode(value ?? string.Empty);
        }
    }
}
