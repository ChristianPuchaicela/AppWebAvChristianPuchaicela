using System;
using WebAppPoliMusicV1.Data;
using WebAppPoliMusicV1.Models;

namespace WebAppPoliMusicV1.Services
{
    /// <summary>Resultado de un intento de autenticacion o registro.</summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public User User { get; set; }

        public static AuthResult Fail(string message)
        {
            return new AuthResult { Success = false, Error = message };
        }

        public static AuthResult Ok(User user)
        {
            return new AuthResult { Success = true, User = user };
        }
    }

    /// <summary>
    /// Reglas de negocio de autenticacion. El rol siempre se obtiene de
    /// TBL_USER.USER_TYPE leido en el servidor.
    /// </summary>
    public static class AuthService
    {
        /// <summary>
        /// Valida las credenciales contra SQL Server. Ademas migra de forma
        /// transparente los hashes heredados (texto plano o SHA-1) a PBKDF2
        /// cuando la contrasena es correcta, conservando la compatibilidad
        /// con las cuentas existentes.
        /// </summary>
        public static AuthResult Authenticate(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                return AuthResult.Fail("Debe ingresar su usuario y su contrasena.");
            }

            User user = UserRepository.GetByUsernameOrEmail(login.Trim());
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
                    UserRepository.UpdatePassword(user.IdUser, upgraded);
                    user.Password = upgraded;
                }
                catch (Exception)
                {
                    // La migracion del hash es una mejora, no un requisito para
                    // iniciar sesion: si falla, el acceso continua.
                }
            }

            return AuthResult.Ok(user);
        }

        /// <summary>
        /// Registra un usuario nuevo con rol normal. Nunca permite que el
        /// propio registro asigne el rol de administrador.
        /// </summary>
        public static AuthResult Register(string username, string email, string password,
                                          DateTime? birthday, string photo)
        {
            username = (username ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return AuthResult.Fail("Usuario, correo y contrasena son obligatorios.");
            }
            if (username.Length < 4 || username.Length > 50)
            {
                return AuthResult.Fail("El usuario debe tener entre 4 y 50 caracteres.");
            }
            if (email.Length > 50)
            {
                return AuthResult.Fail("El correo no puede superar los 50 caracteres.");
            }
            if (password.Length < 8)
            {
                return AuthResult.Fail("La contrasena debe tener al menos 8 caracteres.");
            }
            if (UserRepository.UsernameExists(username, 0))
            {
                return AuthResult.Fail("Ese nombre de usuario ya esta en uso.");
            }
            if (UserRepository.EmailExists(email, 0))
            {
                return AuthResult.Fail("Ese correo ya esta registrado.");
            }

            // El rol se fija en el servidor: el formulario no puede cambiarlo.
            int newId = UserRepository.Create(
                username,
                PasswordHasher.Hash(password),
                email,
                birthday,
                string.IsNullOrEmpty(photo) ? "~/userImages/default.jpg" : photo,
                UserRoles.Normal);

            return AuthResult.Ok(UserRepository.GetById(newId));
        }

        /// <summary>
        /// Cambia la contrasena de un usuario verificando la contrasena actual.
        /// </summary>
        public static AuthResult ChangePassword(int userId, string currentPassword, string newPassword,
                                                 string confirmPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                return AuthResult.Fail("Debe indicar la contrasena actual y la nueva.");
            }
            if (newPassword.Length < 8)
            {
                return AuthResult.Fail("La nueva contrasena debe tener al menos 8 caracteres.");
            }
            if (newPassword != confirmPassword)
            {
                return AuthResult.Fail("La nueva contrasena y su confirmacion no coinciden.");
            }

            User user = UserRepository.GetById(userId);
            if (user == null)
            {
                return AuthResult.Fail("El usuario no existe.");
            }
            if (!PasswordHasher.Verify(currentPassword, user.Password))
            {
                return AuthResult.Fail("La contrasena actual es incorrecta.");
            }

            UserRepository.UpdatePassword(userId, PasswordHasher.Hash(newPassword));
            return AuthResult.Ok(user);
        }
    }
}
