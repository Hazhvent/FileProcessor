namespace FileProcessor.Core.Dto
{
    public class ParametrosCargaDocumentoExterno
    {
        public Int64 IdPoliza { get; set; }
        public Int64 IdCertificado { get; set; }
        public int Firma { get; set; }
        public Double SumaAsegurada { get; set; }

        public string NombreRtf { get; set; } = string.Empty;
        public string Lote { get; set; } = string.Empty;

        public ICollection<Usuario> Usuarios { get; set; } = [];
        public ICollection<Documento> Documentos { get; set; } = [];

        public int IDTIPODOC { get; set; }
        public int ID_PRODUCTO { get; set; }
        public int IdDocumento { get; set; }

        public string RutaDirectorio { get; set; } = string.Empty;
        public string RutaDocumento { get; set; } = string.Empty;
        public string NombrePdf { get; set; } = string.Empty;

        public int ID_USUARIO { get; set; }
        public Int32 Aprobacion { get; set; }

        public string NombreArchivoEliminar { get; set; } = string.Empty;

        public int IdRamo { get; set; }
        public Double Prima { get; set; }

        public string Contratante { get; set; } = string.Empty;

        public int IdMoneda { get; set; }
        public int Usuario { get; set; }

        public string CodTipoDoc { get; set; } = string.Empty;
        public string GeneraSaludo { get; set; } = string.Empty;

        public int TipoConsulta { get; set; }
        public int TipoAccion { get; set; }

    }
}
