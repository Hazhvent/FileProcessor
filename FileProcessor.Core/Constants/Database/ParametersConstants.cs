namespace FileProcessor.Core.Constants.Database
{
    public static class ParametersConstants
    {
        // ==========================================
        // 1. CONFIGURACIÓN DE CORREOS Y SMTP
        // ==========================================
        public const string SMTP_PORT = "SMTP_PORT";
        public const string SMTP_USR = "SMTP_USR";
        public const string SMTP_PWD = "SMTP_PWD";
        public const string SMTP_HOST = "SMTP_HOST";

        // ==========================================
        // CONSTANTES PARA CURSORES DE SALIDA (OUTPUT)
        // ==========================================
        public const string CURSOR = "refCursor";
        public const string CURSOR_SEC = "refCursorSec";
        public const string CURSOR_OP = "refCursorOP";
        public const string CURSOR_CO = "refCursorCo";
        public const string CURSOR_RET = "cursorRet";
        public const string CURSOR_MON = "refCursorMon";
        public const string P_REF_CURSOR = "p_refCursor";

        // ==========================================
        // CONSTANTES PARA PARÁMETROS DE ENTRADA (INPUT)
        // ==========================================

        // Identificadores y Pólizas
        public const string ID_DOCUMENTO = "idDoc";
        public const string PS_ID_DOCUMENTO = "psIdDocumento";
        public const string ID_POLIZA = "idPoliza";
        public const string ID_POL = "idPol";
        public const string PS_POLIZA = "psPoliza";
        public const string P_ID_POLIZA = "p_idPoliza";
        public const string ID_CERTIFICADO = "idCertificado";
        public const string PS_CERTIFICADO = "psCertificado";
        public const string PD_CERTIFICADO = "p_idcertificado";
        public const string P_CERTIFICADO = "p_certificado";

        // Fechas
        public const string FECHA = "fecha";

        // Clientes y Usuarios
        public const string P_SCLIENTE = "scliente";
        public const string P_DNI = "psDNI";
        public const string CLIENTE = "cliente";
        public const string SCLIENT = "SCient";
        public const string P_VSCLIENT = "p_vSCLIENT";
        public const string ID_USUARIO = "idUsuario";
        public const string ID_US = "idUs";
        public const string ID_USUARIO_CREADOR = "idUsuarioCreador";
        public const string ID_US_CREADOR = "idUsCreador";
        public const string ID_USERE = "idusere";

        // Generales, Aprobación y Bitácora
        public const string ESTATUS = "estatus";
        public const string NOMBRE_ARCHIVO = "nombreArchivo";
        public const string NOMBRE_ARCH = "nombreArch";
        public const string NOMBRE_DOCUMENTO = "NOMBREDOCUMENTO";
        public const string RUTA_DOCUMENTO = "RUTADOCUMENTO";
        public const string RUTA = "ruta";
        public const string ID_GENERICO = "idGenerico";
        public const string LOTE = "lote";
        public const string P_LOTE = "pLote";
        public const string ASIG = "asig";
        public const string EMITIDA_VAL = "emitidaVal";
        public const string NOM = "nom";
        public const string NOMBRE_DOC = "nombreDoc";
        public const string MONTO = "monto";
        public const string MONEDA = "moneda";
        public const string RAMO = "ramo";
        public const string PS_RAMO = "psRamo";
        public const string PLANTILLA = "PLANTILLA";
        public const string ID_POLIZA_CAPS = "IDPOLIZA";
        public const string CERTIFICADO_CAPS = "CERTIFICADO";

        // Parámetros de Inserción Específicos
        public const string FIRMA = "firma";
        public const string SUMA = "suma";
        public const string PRODUCTO = "producto";
        public const string CONTRATANTE = "contratante";
        public const string ASEGURADO = "asegurado";
        public const string ID_TIPO_DOC = "idTipoDoc";
        public const string ID_REGLA = "idRegla";
        public const string ID_RTF = "idrtf";
        public const string PS_GENERA_SALUDO = "psGeneraSaludo";
        public const string P_NOMBRE = "pNombre";
        public const string P_APELLIDO = "pApellido";
        public const string P_CORREO = "pCorreo";
        public const string P_DOCUMENTOS = "pDocumentos";
        public const string P_DNI_USER = "pDNI";

        // =========================================================================
        // 🎯 PARÁMETROS DEL "UPDATE" DE PÓLIZA
        // =========================================================================
        public const string ID_CLIENTE = "p_idcliente";
        public const string PRIMA_CERTIFICADO = "p_primacertificado";
        public const string POLI_FEC_EMISION = "p_polifecemision";
        public const string POLI_FEC_VIG = "p_polifecvig";
        public const string MONEDA_CORTO = "p_moneda_corto";
        public const string RUC = "p_ruc";
        public const string DIR = "p_direccion";
        public const string SEX = "p_sexo";
        public const string CORREO = "p_correo";
        public const string TEL = "p_telefono";
        public const string NAC = "p_nacimiento";
        public const string DIR2 = "p_direccionseg";
        public const string PRIMA_POLIZA = "p_primapoliza";
        public const string CERT_FEC_EM = "p_cerfecemision";
        public const string CERT_FEC_VIG = "p_cerfecvig";
        public const string P_NOMBRES = "p_nombres";
        public const string P_ID_TIPO_DOC_UP = "p_idtipodoc";
        public const string P_ID_GENERICO = "p_idgenerico";
        public const string P_ID_MONEDA_UP = "P_idMoneda";

        // Los Andes
        public const string PS_SID_DOC = "psSiddoc";

        // Valores de Control
        public const string TIPO_DNI = "DNI";
        public const string TIPO_RUC = "RUC";
        public const string NROLE_DEFECTO = "2";
    }
}