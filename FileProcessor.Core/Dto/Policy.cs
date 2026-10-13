namespace FileProcessor.Core.Dto
{
    public class Policy
    {
        public string SCERTYPE { get; set; } = string.Empty;
        public int IDRAMO { get; set; }

        public int NPRODUCT { get; set; }

        public Int64 NPOLICY { get; set; }

        public string SCLIENT { get; set; } = string.Empty;

        public double NCAPITAL { get; set; }

        public double NPREMIUM { get; set; }

        public string DISSUEDAT { get; set; } = string.Empty;

        public string DEXPIRDAT { get; set; } = string.Empty;

        public Int64 NCERTIF { get; set; }

        public virtual Client CLIENT { get; set; } = null!;

        public virtual ICollection<Client> ASEGURADOS { get; set; } = [];

        public double Prima { get; set; } ///SIRVE PARA POLIZA Y PARA CERTIFICADO EL CAMPO

        public double PrimaIGV { get; set; }//ES LA QUE SE SACA CON FORMULA EN SQL

        public string Tablaconfigurada { get; set; } = string.Empty;
    }
}
