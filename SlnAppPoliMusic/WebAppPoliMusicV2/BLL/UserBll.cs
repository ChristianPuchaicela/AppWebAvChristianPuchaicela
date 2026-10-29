using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WebAppPoliMusicV2.DAL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2.BLL
{
    /// <summary>
    /// Reglas de negocio de TBL_USER. La capa DAL solo conoce SQL; aqui viven
    /// las validaciones, la unicidad de usuario/correo y la proteccion del
    /// ultimo administrador.
    /// </summary>
    public class UserBll
    {
        private readonly UserDal _dal = new UserDal();

        public List<User> Read()
        {
            return _dal.Read();
        }

        public User ReadById(int idUser)
        {
            return _dal.ReadById(idUser);
        }

        public User ReadByLogin(string login)
        {
            return string.IsNullOrEmpty(login) ? null : _dal.ReadByLogin(login.Trim());
        }

        /// <summary>Valida los campos obligatorios y el formato del correo.</summary>
        public string ValidationsRequired(User user)
        {
            if (user == null)
            {
                return "No se recibieron los datos del usuario.";
            }
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return "El nombre de usuario es obligatorio.";
            }
            if (user.Username.Trim().Length < 4 || user.Username.Trim().Length > Constants.MAX_USERNAME)
            {
                return "El usuario debe tener entre 4 y " + Constants.MAX_USERNAME + " caracteres.";
            }
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return "El correo electronico es obligatorio.";
            }
            if (user.Email.Length > Constants.MAX_EMAIL)
            {
                return "El correo no puede superar los " + Constants.MAX_EMAIL + " caracteres.";
            }
            if (!IsValidEmail(user.Email))
            {
                return "El correo electronico no tiene un formato valido.";
            }
            return string.Empty;
        }

        /// <summary>Comprueba que el usuario y el correo no esten ya registrados.</summary>
        public string ValidationsDuplicated(User user, int excludeId)
        {
            if (_dal.UsernameExists(user.Username.Trim(), excludeId))
            {
                return "Ese nombre de usuario ya esta en uso.";
            }
            if (_dal.EmailExists(user.Email.Trim(), excludeId))
            {
                return "Ese correo ya esta registrado.";
            }
            return string.Empty;
        }

        public OperationResult Save(User user)
        {
            string error = ValidationsRequired(user);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }
            error = ValidationsDuplicated(user, 0);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }
            int id = _dal.Create(user);
            return OperationResult.Ok("Usuario registrado correctamente.", id);
        }

        public OperationResult Update(User user)
        {
            string error = ValidationsRequired(user);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }
            error = ValidationsDuplicated(user, user.IdUser);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }

            // No se permite quedarse sin administradores.
            User current = _dal.ReadById(user.IdUser);
            if (current != null && current.IsAdmin && !user.IsAdmin && _dal.CountAdmins() <= 1)
            {
                return OperationResult.Fail("Debe existir al menos un administrador.");
            }

            _dal.Update(user);
            return OperationResult.Ok("Datos del usuario actualizados.");
        }

        public OperationResult UpdatePassword(int idUser, string storedPassword)
        {
            _dal.UpdatePassword(idUser, storedPassword);
            return OperationResult.Ok("Contrasena actualizada correctamente.");
        }

        public OperationResult Delete(int idUser, int currentUserId)
        {
            User user = _dal.ReadById(idUser);
            if (user == null)
            {
                return OperationResult.Fail("El usuario ya no existe.");
            }
            if (user.IdUser == currentUserId)
            {
                return OperationResult.Fail("No puede eliminar su propia cuenta mientras la usa.");
            }
            if (user.IsAdmin && _dal.CountAdmins() <= 1)
            {
                return OperationResult.Fail("Debe existir al menos un administrador.");
            }
            if (_dal.CountPlaylists(idUser) > 0)
            {
                return OperationResult.Fail("El usuario tiene playlists asociadas. Eliminelas primero.");
            }
            _dal.Delete(idUser);
            return OperationResult.Ok("Usuario eliminado.");
        }

        public int CountAll()
        {
            return _dal.CountAll();
        }

        /// <summary>Validacion de correo sin expresiones regulares fragiles.</summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            return Regex.IsMatch(email.Trim(),
                @"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$",
                RegexOptions.IgnoreCase);
        }
    }
}
