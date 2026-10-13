namespace FileProcessor.Core.Constants.Database
{
    public static class ProceduresConstants
    {
        // 📄 Obtención de Plantillas y Documentos
        public const string SP_GETDATOSDOC = "PKG_PD_WEBSERVICE.SP_GETDATOSDOC";
        public const string SP_GETDOCPORNOMBRE = "PKG_PD_WEBSERVICE.SP_GETDOCPORNOMBRE";
        public const string PD_SP_GETSECCIONESBYID = "PKG_PD_WEBSERVICE.SP_GETSECCIONESBYID";
        public const string SP_GETRAMODOC = "PKG_PD_WEBSERVICE.SP_GETRAMODOC";
        public const string SP_GETPRODUCTOSRAMODOC = "PKG_PD_WEBSERVICE.SP_GETPRODUCTOSRAMODOC";
        public const string SP_GET_CLAVEPDF = "PKG_PD_WEBSERVICE.SP_GET_CLAVEPDF";

        // 🛡️ Datos de Pólizas, Certificados y Clientes (Flujo Normal)
        public const string PD_SP_GETPOLIZAS = "PKG_PD_WEBSERVICE.SP_GETPOLIZAS";
        public const string SP_GETCERTIFICADO = "PKG_PD_WEBSERVICE.SP_GETCERTIFICADO";
        public const string PD_SP_GETCLIENTE = "PKG_PD_WEBSERVICE.SP_GETCLIENTE";
        public const string SP_GETADDRESS = "PKG_PD_WEBSERVICE.SP_GETADDRESS";
        public const string SP_GETDESTINATARIO = "PKG_PD_WEBSERVICE.SP_DESTINATARIO";
        public const string SP_GETMONEDA = "PKG_PD_WEBSERVICE.SP_GETMONEDA";
        public const string SP_PRIMAS = "PKG_PD_WEBSERVICE.SP_PRIMAS";
        public const string SP_GETROLES = "PKG_PD_WEBSERVICE.SP_GETROLES";
        public const string SP_GETIDUSUARIOEXTERNO = "PKG_PD_WEBSERVICE.SP_GETIDUSUARIOEXTERNO";
        public const string SP_GETSOLICITUDCERTIFICADOTAGS = "PKG_PD_WEBSERVICE.SP_GETSOLICITUDCERTIFICADOTAGS";

        // 🏢 Flujo Tabla Intermedia (Caja Los Andes)
        public const string SP_GET_CONFIG_REG_CLIENTES = "PKG_PD_WEBSERVICE.SP_GET_CONFIG_REG_CLIENTES";
        public const string SP_GET_CERTIFICADO_LOS_ANDES = "PKG_PD_WEBSERVICE.SP_GET_CERTIFICADO_LOS_ANDESTAGS";
        public const string SP_GETCLIENTE_LOS_ANDES = "PKG_PD_WEBSERVICE.SP_GETCLIENTE_LOS_ANDES";
        public const string SP_GET_CERTIFTABLAINTERMEDIATAGS = "PKG_PD_WEBSERVICE.SP_GET_CERTIFTABLAINTERMEDIATAGS";

        // ⚙️ Reglas, Revisores y Utilidades
        public const string SP_REGLA = "PKG_PD_WEBSERVICE.SP_REGLA";
        public const string SP_CORREOSREVISOR = "PKG_PD_WEBSERVICE.SP_CORREOSREVISOR";
        public const string SP_CORREOSREVISORGENNORMAL = "PKG_PD_WEBSERVICE.SP_CORREOSREVISORGENNORMAL";
        public const string SP_NUEVO_LOTE = "PKG_PD_WEBSERVICE.SP_NUEVO_LOTE";

        // 💾 Inserciones y Actualizaciones (Escritura)
        public const string SP_POLIZA_EXTERNO = "PKG_PD_WEBSERVICE.SP_POLIZA_EXTERNO";
        public const string SP_ActPoliza = "PKG_PD_WEBSERVICE.SP_ActPoliza";
        public const string SP_POLIZAALTA = "PKG_PD_WEBSERVICE.SP_POLIZAALTA";
        public const string SP_BITACORAALTA = "PKG_PD_WEBSERVICE.SP_BITACORAALTA";
        public const string SP_SINAPROBPOLIZAALTA = "PKG_PD_WEBSERVICE.SP_SINAPROBPOLIZAALTA";
        public const string SP_SINAPROBITACORAALTA = "PKG_PD_WEBSERVICE.SP_SINAPROBITACORAALTA";
        public const string SP_REGISTRARDOCUMENTORECHAZADO = "PKG_PD_WEBSERVICE.SP_REGISTRARDOCUMENTORECHAZADO";
        public const string SP_INSERTUSEXTERNO = "PKG_PD_WEBSERVICE.SP_INSERTUSEXTERNO";

        // 🌐 Rutas Locales, NAS y Configuración de Email
        public const string GET_API_PATHS_DATA = "PKG_PD_WEBSERVICE.GET_API_PATHS_DATA";
        public const string GET_MAIL_DATA = "PKG_PD_WEBSERVICE.GET_MAIL_DATA";
    }
}