using System;
using System.Collections.Generic;
using WebAppPoliMusicV2.DAL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2.BLL
{
    /// <summary>
    /// Reglas de negocio de playlists. Toda operacion de escritura comprueba
    /// la propiedad del dato contra el usuario de la sesion: el identificador
    /// que llega por la URL nunca se usa para decidir permisos.
    /// </summary>
    public class PlaylistBll
    {
        private readonly PlaylistDal _dal = new PlaylistDal();

        public List<Playlist> Read()
        {
            return _dal.Read();
        }

        public List<Playlist> ReadVisibleFor(int idUser)
        {
            return _dal.ReadVisibleFor(idUser);
        }

        public List<Playlist> ReadOwnedBy(int idUser)
        {
            return _dal.ReadOwnedBy(idUser);
        }

        public Playlist ReadOwned(int idPlaylist, int idUser)
        {
            return _dal.ReadOwned(idPlaylist, idUser);
        }

        public Playlist ReadReadable(int idPlaylist, int idUser)
        {
            return _dal.ReadReadable(idPlaylist, idUser);
        }

        public Playlist ReadById(int idPlaylist)
        {
            return _dal.ReadById(idPlaylist);
        }

        public OperationResult Create(int idUser, string title, int type, string photo)
        {
            string error = ValidateTitle(title);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }
            if (type != Constants.ID_PLAYLIST_PUBLIC && type != Constants.ID_PLAYLIST_PRIVATE)
            {
                type = Constants.ID_PLAYLIST_PUBLIC;
            }
            string finalPhoto = string.IsNullOrEmpty(photo) ? Constants.DEFAULT_PLAYLIST_PHOTO : photo;
            int id = _dal.Create(idUser, title.Trim(), DateTime.Now, type, finalPhoto);
            return OperationResult.Ok("Playlist creada correctamente.", id);
        }

        public OperationResult UpdateOwned(int idPlaylist, int idUser, string title, int type, string photo)
        {
            Playlist owned = _dal.ReadOwned(idPlaylist, idUser);
            if (owned == null)
            {
                return OperationResult.Fail("La playlist no existe o no le pertenece.");
            }
            string error = ValidateTitle(title);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }
            string finalPhoto = string.IsNullOrEmpty(photo) ? owned.Photo : photo;
            _dal.UpdateOwned(idPlaylist, idUser, title.Trim(), type, finalPhoto);
            return OperationResult.Ok("Playlist actualizada correctamente.");
        }

        public OperationResult DeleteOwned(int idPlaylist, int idUser)
        {
            if (!_dal.DeleteOwned(idPlaylist, idUser))
            {
                return OperationResult.Fail("La playlist no existe o no le pertenece.");
            }
            return OperationResult.Ok("Playlist eliminada.");
        }

        /// <summary>Eliminacion administrativa: no comprueba propiedad.</summary>
        public OperationResult Delete(int idPlaylist)
        {
            if (_dal.ReadById(idPlaylist) == null)
            {
                return OperationResult.Fail("La playlist no existe.");
            }
            _dal.DeleteCascade(idPlaylist);
            return OperationResult.Ok("Playlist eliminada.");
        }

        public OperationResult AddSong(int idPlaylist, int idUser, int idSong)
        {
            if (_dal.ReadOwned(idPlaylist, idUser) == null)
            {
                return OperationResult.Fail("Solo el propietario puede modificar esta playlist.");
            }
            if (_dal.ContainsSong(idPlaylist, idSong))
            {
                return OperationResult.Fail("La cancion ya esta en la playlist.");
            }
            _dal.AddSong(idPlaylist, idSong);
            return OperationResult.Ok("Cancion agregada a la playlist.");
        }

        public OperationResult RemoveSong(int idPlaylist, int idUser, int idSong)
        {
            if (_dal.ReadOwned(idPlaylist, idUser) == null)
            {
                return OperationResult.Fail("Solo el propietario puede modificar esta playlist.");
            }
            _dal.RemoveSong(idPlaylist, idSong);
            return OperationResult.Ok("Cancion quitada de la playlist.");
        }

        public int CountAll()
        {
            return _dal.CountAll();
        }

        private static string ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return "El titulo de la playlist es obligatorio.";
            }
            if (title.Trim().Length > Constants.MAX_TITLE)
            {
                return "El titulo no puede superar los " + Constants.MAX_TITLE + " caracteres.";
            }
            return string.Empty;
        }
    }
}
