namespace FileProcessor.Core.Dto
{
    public class CertificadoCampoFe
    {
        public Int64 NumeroPoliza { get; set; }
        public Int64 Certificado { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVigencia { get; set; }
        public DateTime FechaFinVigencia { get; set; }
        public string NombreAsegurado { get; set; } = string.Empty;
        public string DocumentoAsegurado { get; set; } = string.Empty;
        public string TelefonoAsegurado { get; set; } = string.Empty;
        public string EmailAsegurado { get; set; } = string.Empty;
        public string SexoAsegurado { get; set; } = string.Empty;
        public DateTime FechaNacimientoAsegurado { get; set; }
        public int EdadAsegurado { get; set; }
        public string DireccionAsegurado { get; set; } = string.Empty;
        public string NombreBeneficiario { get; set; } = string.Empty;
        public string DocumentoBeneficiario { get; set; } = string.Empty;
        public string RelacionBeneficiario { get; set; } = string.Empty;
        public string TipoDocAsegurado { get; set; } = string.Empty;
        public string TipoDocBeneficiario { get; set; } = string.Empty;
        public string SClientAsegurado { get; set; } = string.Empty;
        public string SClientBeneficiario { get; set; } = string.Empty;
        public int NModulec { get; set; }
    }
}
