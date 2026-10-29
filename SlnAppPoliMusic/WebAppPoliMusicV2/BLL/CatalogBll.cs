using System.Collections.Generic;
using WebAppPoliMusicV2.DAL;
using WebAppPoliMusicV2.Entities;
using WebAppPoliMusicV2.Util;

namespace WebAppPoliMusicV2.BLL
{
    /// <summary>
    /// Acceso al dominio de TBL_CATALOG. Evita duplicar en el codigo los
    /// valores de tipo de usuario y de tipo de playlist.
    /// </summary>
    public class CatalogBll
    {
        private readonly CatalogDal _dal = new CatalogDal();

        public List<CatalogItem> ReadAll()
        {
            return _dal.ReadAll();
        }

        public List<CatalogItem> ReadUserTypes()
        {
            return _dal.ReadByParent(Constants.CATALOG_USER_TYPE);
        }

        public List<CatalogItem> ReadPlaylistTypes()
        {
            return _dal.ReadByParent(Constants.CATALOG_PLAYLIST_TYPE);
        }
    }
}
