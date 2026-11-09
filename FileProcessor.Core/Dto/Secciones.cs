namespace FileProcessor.Core.Dto
{
    public class Secciones
    {
        public int IdSeccion { get; set; }
        public string NombreSeccion { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public int IdDocumento { get; set; }
    }
}
