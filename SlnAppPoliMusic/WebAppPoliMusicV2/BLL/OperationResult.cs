namespace WebAppPoliMusicV2.BLL
{
    /// <summary>
    /// Resultado uniforme de una operacion de negocio. Permite que las
    /// paginas muestren el mensaje adecuado sin repetir validaciones.
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Id { get; set; }

        public static OperationResult Ok(string message)
        {
            return new OperationResult { Success = true, Message = message };
        }

        public static OperationResult Ok(string message, int id)
        {
            return new OperationResult { Success = true, Message = message, Id = id };
        }

        public static OperationResult Fail(string message)
        {
            return new OperationResult { Success = false, Message = message };
        }
    }
}
