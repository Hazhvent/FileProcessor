namespace FileProcessor.Core.Dto
{
    public class Polizas
    {
        public long IdPoliza { get; set; }
        public int Firmadigital { get; set; }

        public int IDTBLGENERICA { get; set; }
        public string PsMensaje { get; set; } = string.Empty;
        public string PsNombreDocumento { get; set; } = string.Empty;
        public string PsRutaRTF { get; set; } = string.Empty;
    }
}
