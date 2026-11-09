namespace FileProcessor.Core.Dto
{
    public class DocumentoRTF
    {
        public int IDDOCUMENTO { get; set; }
        public string NOMBREDOCUMENTO { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public string RUTARTF { get; set; } = string.Empty;
    }
}
