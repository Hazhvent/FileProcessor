namespace FileProcessor.Core.Dto
{
    public class PolizaLosAndes
    {
        public Int64 Npolicy { get; set; }
        public Int64 Ncertif { get; set; }
        public string Siddoc { get; set; } = string.Empty;
        public double NCAPITAL { get; set; }
        public double NPREMIUM { get; set; }
        public string NRole { get; set; } = string.Empty;
        public string FechaEmision { get; set; } = string.Empty;
    }
}