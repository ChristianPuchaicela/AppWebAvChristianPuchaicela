using System;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2.BLL
{
    /// <summary>Resultado de una operacion de autenticacion o registro.</summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public User User { get; set; }

        public static AuthResult Ok(User user, string message)
        {
            return new AuthResult { Success = true, User = user, Message = message };
        }

        public static AuthResult Fail(string message)
        {
            return new AuthResult { Success = false, Message = message };
        }
    }

    /// <summary>
    /// Reglas de negocio de autenticacion. El rol se obtiene siempre de
    /// TBL_USER.USER_TYPE leido en el servidor, nunca del formulario.
    /// </summary>
    public class AuthService
    {
        private readonly UserBll _users = new UserBll();

        /// <summary>
        /// Valida las credenciales contra SQL Server y migra de forma
        /// transparente los hashes heredados (texto plano o SHA-1) a PBKDF2
        /// cuando la contrasena es correcta.
        /// </summary>
        public AuthResult Authenticate(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password))
            {
                return AuthResult.Fail("Debe ingresar su usuario y su contrasena.");
            }

            User user = _users.ReadByLogin(login.Trim());
            if (user == null)
            {
                // Mensaje generico: no se revela si el usuario existe.
                return AuthResult.Fail("Usuario o contrasena incorrectos.");
            }

            if (!PasswordHasher.Verify(password, user.Password))
            {
                return AuthResult.Fail("Usuario o contrasena incorrectos.");
            }

            if (PasswordHasher.NeedsUpgrade(user.Password))
            {
                try
                {
                    string upgraded = PasswordHasher.Hash(password);
                    _users.UpdatePassword(user.IdUser, upgraded);
                    user.Password = upgraded;
                }
                catch (Exception)
                {
                    // La migracion del hash es una mejora, no un requisito para
                    // iniciar sesion: si falla, el acceso continua.
                }
            }

            return AuthResult.Ok(user, "Bienvenido de nuevo, " + user.Username + ".");
        }

        /// <summary>
        /// Registra un usuario nuevo con rol normal. El formulario no puede
        /// asignar el rol de administrador.
        /// </summary>
        public AuthResult Register(string username, string email, string password,
                                   string confirmPassword, DateTime? birthday, string photo)
        {
            username = (username ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return AuthResult.Fail("Usuario, correo y contrasena son obligatorios.");
            }
            if (password.Length < Constants.MIN_PASSWORD)
            {
                return AuthResult.Fail("La contrasena debe tener al menos " + Constants.MIN_PASSWORD + " caracteres.");
            }
            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                return AuthResult.Fail("La contrasena y su confirmacion no coinciden.");
            }
            if (birthday.HasValue && birthday.Value.Date > DateTime.Today)
            {
                return AuthResult.Fail("La fecha de nacimiento no puede ser futura.");
            }

            User user = new User
            {
                Username = username,
                Email = email,
                Password = PasswordHasher.Hash(password),
                Birthday = birthday,
                UserPhoto = string.IsNullOrEmpty(photo) ? Constants.DEFAULT_USER_PHOTO : photo,
                UserType = Constants.ID_USER_NORMAL
            };

            OperationResult result = _users.Save(user);
            if (!result.Success)
            {
                return AuthResult.Fail(result.Message);
            }

            User created = _users.ReadById(result.Id);
            return AuthResult.Ok(created, "Cuenta creada correctamente. Ya puede iniciar sesion.");
        }

        /// <summary>Cambia la contrasena verificando la contrasena actual.</summary>
        public AuthResult ChangePassword(int idUser, string currentPassword, string newPassword,
                                        string confirmPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                return AuthResult.Fail("Debe indicar la contrasena actual y la nueva.");
            }
            if (newPassword.Length < Constants.MIN_PASSWORD)
            {
                return AuthResult.Fail("La nueva contrasena debe tener al menos " + Constants.MIN_PASSWORD + " caracteres.");
            }
            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                return AuthResult.Fail("La nueva contrasena y su confirmacion no coinciden.");
            }

            User user = _users.ReadById(idUser);
            if (user == null)
            {
                return AuthResult.Fail("El usuario no existe.");
            }
            if (!PasswordHasher.Verify(currentPassword, user.Password))
            {
                return AuthResult.Fail("La contrasena actual es incorrecta.");
            }

            _users.UpdatePassword(idUser, PasswordHasher.Hash(newPassword));
            return AuthResult.Ok(user, "Contrasena actualizada correctamente.");
        }
    }
}
