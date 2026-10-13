using FileProcessor.Core.Dto;

namespace FileProcessor.Core.Interfaces.Repositories
{
    public interface IRepository
    {
        // ==============================================================================
        // 📄 Obtención de Plantillas y Documentos
        // ==============================================================================
        Task<ParametrosCargaDocumentoExterno?> GetDatosDocAsync(string nom);
        Task<DocumentoRTF?> ObtenerDocumentoRTFAsync(string nombreDoc);
        Task<IEnumerable<Secciones>> GetSeccionesByIdAsync(int idDocumento);
        Task<Policy?> GetRamoDocAsync(int idDoc);
        Task<IEnumerable<Policy>> GetProductosAsync(int idDoc);
        Task<Client?> GetClavePdfAsync(string sClient);

        // ==============================================================================
        // 🛡️ Datos de Pólizas, Certificados y Clientes (Flujo Normal)
        // ==============================================================================
        Task<Policy?> GetPolizaAsync(long idPoliza);
        Task<Policy?> GetCertificadoAsync(long poliza, long idCertificado);
        Task<Client?> GetDataClientAsync(string sclient);
        Task<IEnumerable<Address>> GetAddressAsync(string cliente);
        Task<Client?> GetDataDestinatarioAsync(string dni);
        Task<Moneda?> GetMonedaAsync(long poliza, int certificado);
        Task<Life?> GetPrimasAsync(long poliza, long certificado);
        Task<RolesUsuario?> GetRolesAsync(long poliza, long certificado);
        Task<ParametrosCargaDocumentoExterno?> GetIdUsereAsync(string idusere);
        Task<CertificadoCampoFe?> SolicitudCertificadoAsync(long idPoliza, long certificado);

        // ==============================================================================
        // 🏢 Flujo Tabla Intermedia (Caja Los Andes)
        // ==============================================================================
        Task<Policy?> GetPolizaLosAndesAsync(long psPoliza, int psRamo);
        Task<CertificadoCajaLosAndes?> GetCertificadoTablaIntermediaTagsAsync(long idPoliza, long certificado, string sClient);
        Task<PolizaLosAndes?> GetCertificadoTablaIntermediaAsync(string psTablaconfigurada, long psPoliza, long psCertificado);
        Task<Client?> GetDataClientCertTablaIntermediaAsync(string psSiddoc);

        // ==============================================================================
        // ⚙️ Reglas, Revisores y Utilidades
        // ==============================================================================
        Task<ReglaAprobacion?> GetReglaAsync(double monto, int moneda);
        Task<IEnumerable<Correos>> GetCorreosRevisorAsync(double monto, int moneda);
        Task<IEnumerable<Correos>> GetCorreosRevisorGenNormalAsync(double monto, string poliza, long certificado);
        Task<int> GetNuevoLoteAsync();

        // ==============================================================================
        // 💾 Inserciones y Actualizaciones (Escritura)
        // ==============================================================================
        Task<Polizas?> SetPolizaAsync(long idPoliza, int firma, int ramo, double suma, int producto, DateTime fecha, string contratante, string asegurado, string lote, int idTipoDocumento, int idRegla, int idDocumento, long idcertificado, string psGeneraSaludo);
        Task<bool> UpdateAsync(string idcliente, int primacertificado, string polifecemision, string polifecvig, string monedaCorto, string ruc, string dir, string nom, string sex, string correo, string tel, string nac, string dir2, int primaPoliza, string certfecem, string certfecvig, int idtipodoc, int idgen, int idMoneda);
        Task<Aprobacion?> SetAprobacionAsync(long idPoliza, int idUsuario, int estatus, string nombreArchivo, int idGenerico, int idUsuarioCreador, int asignada, int emitida);
        Task<Aprobacion?> SetSinAprobacionAsync(long idPoliza, int idUsuario, int estatus, string nombreArchivo, int idGenerico, int idUsuarioCreador, int emitida);
        Task<Bitacora?> SetBitacoraAsync(long idPoliza, string ruta, int estatus, string nombreArchivo, int idGenerico, int idUsuarioCreador, int idUsuario);
        Task<Bitacora?> SetSinAprobacionBitacoraAsync(long idPoliza, string ruta, int estatus, string nombreArchivo, int idGenerico, int idUsuarioCreador, int idUsuario);
        Task RegistrarDocumentoRechazadoAsync(string nombreDocumento, string rutaDocumento, long idPoliza, long certificado, string plantilla);
        Task InsertUsuariosAsync(string lote, Usuario usuario, string documentos);

        // ==============================================================================
        // 🌐 Rutas Locales, NAS y Configuración de Email
        // ==============================================================================
        Task<IEnumerable<Parametros>> GetApiPathsDataAsync();
        Task<IEnumerable<Parametros>> GetMailDataAsync();
    }
}