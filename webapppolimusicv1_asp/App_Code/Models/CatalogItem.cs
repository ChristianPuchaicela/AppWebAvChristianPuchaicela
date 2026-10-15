namespace WebAppPoliMusicV1.Models
{
    /// <summary>
    /// Fila generica de [dbo].[TBL_CATALOG]. La tabla es auto-referenciada
    /// mediante COD_CATALOG_PARENT / COD_CATALOG.
    /// </summary>
    public class CatalogItem
    {
        public int IdCatalog { get; set; }
        public string CodCatalogParent { get; set; }
        public string CodCatalog { get; set; }
        public string Value { get; set; }
    }
}
