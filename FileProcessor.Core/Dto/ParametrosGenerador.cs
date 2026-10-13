namespace FileProcessor.Core.Dto
{
    public class ParametrosGenerador
    {
        public int IdPoliza { get; set; }
        public int IdCertificado { get; set; }
        public int Aprobacion { get; set; }
        public int Firma { get; set; }
        public string lote { get; set; } = string.Empty;
        public int usuario { get; set; }
        public int IdTipoDocumento { get; set; }
        public int IdTipoDestinatario { get; set; }
        public int IdTipoOperacion { get; set; }
        public int IdProducto { get; set; }
        public string FechaInicio { get; set; } = string.Empty;
        public string FechaFin { get; set; } = string.Empty;
        public string FechaEmision { get; set; } = string.Empty;
    }
}
