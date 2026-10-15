using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WebAppPoliMusicV1.Models;

namespace WebAppPoliMusicV1.Data
{
    /// <summary>
    /// Lectura de [dbo].[TBL_CATALOG]. Esta tabla define, entre otros dominios:
    ///   PARENT '1' -> USER TYPE      (0 = USER, 1 = ADMIN)
    ///   PARENT '2' -> PLAYLIST TYPE  (0 = Public, 1 = Private)
    /// Se lee para no duplicar el dominio en el codigo.
    /// </summary>
    public static class CatalogRepository
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

        public static List<CatalogItem> GetByParent(string parentCode)
        {
            return Db.Query(
                "SELECT ID_CATALOG, COD_CATALOG_PARENT, COD_CATALOG, VALUE FROM TBL_CATALOG " +
                "WHERE COD_CATALOG_PARENT = @p ORDER BY COD_CATALOG",
                Map, Db.P("@p", parentCode));
        }

        public static List<CatalogItem> GetAll()
        {
            return Db.Query(
                "SELECT ID_CATALOG, COD_CATALOG_PARENT, COD_CATALOG, VALUE FROM TBL_CATALOG ORDER BY ID_CATALOG",
                Map);
        }
    }
}
