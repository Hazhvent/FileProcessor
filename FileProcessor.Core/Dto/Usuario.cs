namespace FileProcessor.Core.Dto
{
    public class Usuario
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public List<DocumentoRef> Documentos { get; set; } = [];
    }

    public class DocumentoRef
    {
        public string Nombre { get; set; } = string.Empty;
    }
}