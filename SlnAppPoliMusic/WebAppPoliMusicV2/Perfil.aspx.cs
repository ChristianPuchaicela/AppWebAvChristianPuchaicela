using System;
using System.Web;
using WebAppPoliMusicV2.BLL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2
{
    /// <summary>
    /// Perfil del usuario autenticado: actualizacion de datos personales y
    /// cambio de contrasena. El rol (USER_TYPE) se conserva siempre desde la
    /// sesion y no se acepta como dato del formulario.
    /// </summary>
    public partial class Perfil : PageBase
    {
        protected string Username;
        protected string Email;
        protected string RoleName;
        protected string BirthdayText;
        protected string BirthdayInput;
        protected string AgeText;
        protected string PhotoUrl;
        protected int PlaylistCount;
        protected string ErrorMessage;

        private readonly UserBll _users = new UserBll();
        private readonly PlaylistBll _playlists = new PlaylistBll();

        public Perfil()
        {
            ActiveSection = "profile";
            ErrorMessage = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                ProcessPost();
                return;
            }
            LoadProfile();
        }

        private void ProcessPost()
        {
            if (!ValidateCsrf())
            {
                ErrorMessage = "La sesion del formulario expiro. Recargue la pagina e intente de nuevo.";
                LoadProfile();
                return;
            }

            string action = FormStr("action");
            if (action == "password")
            {
                ChangePassword();
                return;
            }
            SaveProfile();
        }

        private void SaveProfile()
        {
            User current = _users.ReadById(CurrentUserId);
            if (current == null)
            {
                RedirectWithFlash("~/Logout.aspx", "Su usuario ya no existe.", true);
                return;
            }

            current.Username = FormStr("username");
            current.Email = FormStr("email");
            current.Birthday = FormDate("birthday");

            HttpPostedFile file = Request.Files["photo"];
            if (file != null && file.ContentLength > 0)
            {
                string error;
                string photo = FileStorage.SaveImage(file, "~/UserImages/default.jpg", current.Username, out error);
                if (error != null)
                {
                    ErrorMessage = error;
                    LoadProfile();
                    return;
                }
                current.UserPhoto = photo;
            }

            OperationResult result = _users.Update(current);
            if (!result.Success)
            {
                ErrorMessage = result.Message;
                LoadProfile();
                return;
            }

            // Se refresca la sesion para que la barra superior muestre los cambios.
            UserSession.Refresh(_users.ReadById(current.IdUser));
            RedirectWithFlash("~/Perfil.aspx", result.Message, false);
        }

        private void ChangePassword()
        {
            AuthResult result = new AuthService().ChangePassword(
                CurrentUserId, FormStr("current"), FormStr("newpass"), FormStr("confirmpass"));

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                LoadProfile();
                return;
            }

            RedirectWithFlash("~/Perfil.aspx", result.Message, false);
        }

        private void LoadProfile()
        {
            User user = _users.ReadById(CurrentUserId);
            if (user == null)
            {
                RedirectWithFlash("~/Logout.aspx", "Su usuario ya no existe.", true);
                return;
            }

            Username = user.Username;
            Email = user.Email;
            RoleName = user.RoleName;
            BirthdayText = user.Birthday.HasValue ? user.Birthday.Value.ToString("dd/MM/yyyy") : string.Empty;
            BirthdayInput = user.Birthday.HasValue ? user.Birthday.Value.ToString("yyyy-MM-dd") : string.Empty;
            AgeText = user.Age.HasValue ? user.Age.Value + " a\u00F1os" : string.Empty;
            PhotoUrl = MediaResolver.ResolveWithFallback(user.UserPhoto, Constants.DEFAULT_USER_PHOTO);
            PlaylistCount = _playlists.ReadOwnedBy(user.IdUser).Count;
        }
    }
}
