using System;
using System.Collections.Generic;
using WebAppPoliMusicV2.DAL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2.BLL
{
    /// <summary>Reglas de negocio de canciones y de sus relaciones.</summary>
    public class SongBll
    {
        private readonly SongDal _dal = new SongDal();

        public List<Song> Read(string search)
        {
            return _dal.Read(string.IsNullOrWhiteSpace(search) ? null : search.Trim());
        }

        public Song ReadById(int idSong)
        {
            return _dal.ReadById(idSong);
        }

        public List<Song> ReadMostPlayed(int top)
        {
            return _dal.ReadMostPlayed(top);
        }

        public List<Song> ReadByAlbum(int idAlbum)
        {
            return _dal.ReadByAlbum(idAlbum);
        }

        public List<Song> ReadByArtist(int idArtist)
        {
            return _dal.ReadByArtist(idArtist);
        }

        public List<Song> ReadByPlaylist(int idPlaylist)
        {
            return _dal.ReadByPlaylist(idPlaylist);
        }

        public OperationResult Save(string songName, string songPath, int plays, int idArtist, int idAlbum)
        {
            string error = Validate(songName, songPath);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }

            int id = _dal.Create(songName.Trim(), songPath, plays);
            if (idArtist > 0)
            {
                _dal.SetArtist(id, idArtist);
            }
            if (idAlbum > 0)
            {
                _dal.SetAlbum(id, idAlbum);
            }
            return OperationResult.Ok("Cancion registrada correctamente.", id);
        }

        public OperationResult Update(int idSong, string songName, string songPath, int plays,
                                      int idArtist, int idAlbum)
        {
            Song existing = _dal.ReadById(idSong);
            if (existing == null)
            {
                return OperationResult.Fail("La cancion no existe.");
            }
            string error = Validate(songName, songPath);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }

            // Si el administrador no sube un archivo nuevo se conserva la ruta anterior.
            string finalPath = string.IsNullOrEmpty(songPath) ? existing.SongPath : songPath;
            _dal.Update(idSong, songName.Trim(), finalPath, plays);

            if (idArtist > 0)
            {
                _dal.SetArtist(idSong, idArtist);
            }
            if (idAlbum > 0)
            {
                _dal.SetAlbum(idSong, idAlbum);
            }
            return OperationResult.Ok("Cancion actualizada correctamente.");
        }

        public OperationResult Delete(int idSong)
        {
            Song existing = _dal.ReadById(idSong);
            if (existing == null)
            {
                return OperationResult.Fail("La cancion no existe.");
            }
            _dal.DeleteCascade(idSong);
            return OperationResult.Ok("Cancion eliminada junto con sus vinculos.");
        }

        public void IncrementPlays(int idSong)
        {
            if (idSong > 0)
            {
                _dal.IncrementPlays(idSong);
            }
        }

        public int CountAll()
        {
            return _dal.CountAll();
        }

        public int ReadArtistIdOfSong(int idSong)
        {
            return _dal.ReadArtistIdOfSong(idSong);
        }

        public int ReadAlbumIdOfSong(int idSong)
        {
            return _dal.ReadAlbumIdOfSong(idSong);
        }

        private static string Validate(string songName, string songPath)
        {
            if (string.IsNullOrWhiteSpace(songName))
            {
                return "El nombre de la cancion es obligatorio.";
            }
            if (songName.Trim().Length > Constants.MAX_SONG_NAME)
            {
                return "El nombre no puede superar los " + Constants.MAX_SONG_NAME + " caracteres.";
            }
            if (!string.IsNullOrEmpty(songPath) && songPath.Length > 255)
            {
                return "La ruta del archivo es demasiado larga.";
            }
            return string.Empty;
        }
    }
}
