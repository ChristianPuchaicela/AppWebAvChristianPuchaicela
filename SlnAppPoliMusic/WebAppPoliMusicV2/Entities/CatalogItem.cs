namespace WebAppPoliMusicV2.Entities
{
    /// <summary>
    /// Fila de [dbo].[TBL_CATALOG]. La tabla guarda los dominios de la
    /// aplicacion: COD_CATALOG_PARENT '1' = tipo de usuario,
    /// '2' = tipo de playlist.
    /// </summary>
    public class CatalogItem
    {
        public int IdCatalog { get; set; }
        public string CodCatalogParent { get; set; }
        public string CodCatalog { get; set; }
        public string Value { get; set; }
    }
}
