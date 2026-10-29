using System;
using System.Collections.Generic;
using WebAppPoliMusicV2.DAL;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2.BLL
{
    /// <summary>Reglas de negocio de albumes.</summary>
    public class AlbumBll
    {
        private readonly AlbumDal _dal = new AlbumDal();

        public List<Album> Read(string search)
        {
            return _dal.Read(string.IsNullOrWhiteSpace(search) ? null : search.Trim());
        }

        public Album ReadById(int idAlbum)
        {
            return _dal.ReadById(idAlbum);
        }

        public OperationResult Save(Album album)
        {
            string error = Validate(album);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }
            int id = _dal.Create(album);
            return OperationResult.Ok("Album registrado correctamente.", id);
        }

        public OperationResult Update(Album album)
        {
            if (_dal.ReadById(album.IdAlbum) == null)
            {
                return OperationResult.Fail("El album no existe.");
            }
            string error = Validate(album);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }
            _dal.Update(album);
            return OperationResult.Ok("Album actualizado correctamente.");
        }

        public OperationResult Delete(int idAlbum)
        {
            if (_dal.ReadById(idAlbum) == null)
            {
                return OperationResult.Fail("El album no existe.");
            }
            _dal.DeleteCascade(idAlbum);
            return OperationResult.Ok("Album eliminado junto con su lista de canciones.");
        }

        public int CountAll()
        {
            return _dal.CountAll();
        }

        private static string Validate(Album album)
        {
            if (album == null || string.IsNullOrWhiteSpace(album.TitleAlbum))
            {
                return "El titulo del album es obligatorio.";
            }
            if (album.TitleAlbum.Trim().Length > 50)
            {
                return "El titulo no puede superar los 50 caracteres.";
            }
            if (album.ReleaseYear.HasValue && (album.ReleaseYear.Value < 1900 || album.ReleaseYear.Value > 2100))
            {
                return "El anio de lanzamiento debe estar entre 1900 y 2100.";
            }
            return string.Empty;
        }
    }
}
