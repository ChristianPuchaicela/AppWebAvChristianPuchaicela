using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV2.Entities;

namespace WebAppPoliMusicV2.DAL
{
    /// <summary>
    /// Lectura de [dbo].[TBL_CATALOG], que define los dominios de la
    /// aplicacion: PARENT '1' = tipo de usuario, PARENT '2' = tipo de playlist.
    /// </summary>
    public class CatalogDal
    {
        private static CatalogItem Map(SqlDataReader r)
        {
            return new CatalogItem
            {
                IdCatalog = Db.ToInt(r, "ID_CATALOG"),
                CodCatalogParent = Db.ToStr(r, "COD_CATALOG_PARENT"),
                CodCatalog = Db.ToStr(r, "COD_CATALOG"),
                Value = Db.ToStr(r, "VALUE")
            };
        }

        public List<CatalogItem> ReadAll()
        {
            return Db.Query(
                "SELECT ID_CATALOG, COD_CATALOG_PARENT, COD_CATALOG, VALUE " +
                "FROM TBL_CATALOG ORDER BY ID_CATALOG", Map);
        }

        public List<CatalogItem> ReadByParent(string parentCode)
        {
            return Db.Query(
                "SELECT ID_CATALOG, COD_CATALOG_PARENT, COD_CATALOG, VALUE " +
                "FROM TBL_CATALOG WHERE COD_CATALOG_PARENT = @p ORDER BY COD_CATALOG",
                Map, Db.P("@p", parentCode));
        }
    }
}
