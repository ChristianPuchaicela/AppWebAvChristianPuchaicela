using System;
using System.Collections.Generic;
using WebAppPoliMusicV2.DAL;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2.BLL
{
    /// <summary>Reglas de negocio de artistas.</summary>
    public class ArtistBll
    {
        private readonly ArtistDal _dal = new ArtistDal();

        public List<Artist> Read(string search)
        {
            return _dal.Read(string.IsNullOrWhiteSpace(search) ? null : search.Trim());
        }

        public Artist ReadById(int idArtist)
        {
            return _dal.ReadById(idArtist);
        }

        public OperationResult Save(Artist artist)
        {
            string error = Validate(artist);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }
            int id = _dal.Create(artist);
            return OperationResult.Ok("Artista registrado correctamente.", id);
        }

        public OperationResult Update(Artist artist)
        {
            if (_dal.ReadById(artist.IdArtist) == null)
            {
                return OperationResult.Fail("El artista no existe.");
            }
            string error = Validate(artist);
            if (error.Length > 0)
            {
                return OperationResult.Fail(error);
            }
            _dal.Update(artist);
            return OperationResult.Ok("Artista actualizado correctamente.");
        }

        public OperationResult Delete(int idArtist)
        {
            if (_dal.ReadById(idArtist) == null)
            {
                return OperationResult.Fail("El artista no existe.");
            }
            _dal.DeleteCascade(idArtist);
            return OperationResult.Ok("Artista eliminado junto con sus vinculos.");
        }

        public int CountAll()
        {
            return _dal.CountAll();
        }

        private static string Validate(Artist artist)
        {
            if (artist == null || string.IsNullOrWhiteSpace(artist.ArtistName))
            {
                return "El nombre del artista es obligatorio.";
            }
            if (artist.ArtistName.Trim().Length > 100)
            {
                return "El nombre no puede superar los 100 caracteres.";
            }
            if (!string.IsNullOrEmpty(artist.ArtistLastname) && artist.ArtistLastname.Trim().Length > 100)
            {
                return "El apellido no puede superar los 100 caracteres.";
            }
            return string.Empty;
        }
    }
}
