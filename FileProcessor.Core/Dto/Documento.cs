using System.Text.Json.Serialization;

namespace FileProcessor.Core.Dto
{
    public class Documento
    {
        public string Nombre { get; set; } = string.Empty;
        public string Poliza { get; set; } = string.Empty;
        public string Certificado { get; set; } = string.Empty;
        public string SumaAsegurada { get; set; } = string.Empty;
        public string Prima { get; set; } = string.Empty;
        public string NombrePlantilla { get; set; } = string.Empty;
        public string Ramo { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public string DocContratante { get; set; } = string.Empty;

        [JsonIgnore]
        public int IdTipoAccion { get; set; }

        [JsonIgnore]
        public int IdTipoConsulta { get; set; }

        private string _generaSaludo = "N";
        public string GeneraSaludo
        {
            get => _generaSaludo;
            set => _generaSaludo = string.IsNullOrWhiteSpace(value) ? "N" : value.Trim().ToUpper();
        }
    }
}