namespace FileProcessor.Core.Dto
{
    public class CertificadoCajaLosAndes
    {
        public Int64 NumeroPoliza { get; set; }
        public Int64 Certificado { get; set; }
        public DateTime FechaEmision { get; set; }
        public string NombreAsegurado { get; set; } = string.Empty;
        public string DireccionAsegurado { get; set; } = string.Empty;
        public string SClientAsegurado { get; set; } = string.Empty;
        public string DocumentoAsegurado { get; set; } = string.Empty;
        public string NombreBeneficiario1 { get; set; } = string.Empty;
        public string NombreBeneficiario2 { get; set; } = string.Empty;
        public string DireccionBeneficiario1 { get; set; } = string.Empty;
        public string DireccionBeneficiario2 { get; set; } = string.Empty;
        public string RelacionBeneficiario1 { get; set; } = string.Empty;
        public string RelacionBeneficiario2 { get; set; } = string.Empty;
        public string PorcentajeBeneficiario1 { get; set; } = string.Empty;
        public string PorcentajeBeneficiario2 { get; set; } = string.Empty;
        public string PlanCertificado { get; set; } = string.Empty;
    }
}
