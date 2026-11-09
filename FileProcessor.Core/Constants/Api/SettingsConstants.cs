namespace FileProcessor.Core.Constants.Api
{
    // [no tocar 🔒] 
    public static class SettingsConstants
    {
        // Nombres de secciones de configuración
        public const string DB_CONNECTION_STRING_NAME = "Database";
        public const string APPLICATION_SETTINGS_SECTION_NAME = "ApplicationSettings";
        public const string NAS_SETTINGS_SECTION_NAME = "NASSettings";
        public const string PRIORITY_WORDS_SECTION_NAME = "PriorityWordsSettings";
        public const string SOAP_SETTINGS_SECTION_NAME = "SoapSettings";
        public const string SOAP_SETTINGS_ENDPOINT_PATH_KEY = $"{SOAP_SETTINGS_SECTION_NAME}:EndpointPath";

        // Tipos de contenido HTTP
        public const string HTTP_CONTENT_TYPE_JSON = "application/json";

        // Configuración de Correo
        public const string EMAIL_SENDER_DISPLAY_NAME = "File Processor System";
        public const string EMAIL_SUBJECT_PENDING_APPROVAL = "Pólizas pendientes de aprobación";
        public const string EMAIL_BODY_PENDING_APPROVAL_TEMPLATE = "Tienes el lote '{0}' de pólizas en espera de aprobación.";

        // 👤 Usuarios, Actores y Empresas del Sistema
        public const string DEFAULT_SYSTEM_USER = "ADMINPD";

        // Valores por defecto para Poliza
        public const int DEFAULT_POLIZA_ASIGNADA = 1;
        public const int DEFAULT_POLIZA_EMITIDA = 1;

        // Extension de archivos, Plantillas Especiales, rutas, etc
        public const string RTF_FILE_NAME_TEMPLATE = "{0}.rtf";
        public const string PDF_FILE_NAME_TEMPLATE = "{0}.pdf";
        public const string RTF_PDF_NAME_PATTERN = "{0}_{1}_{2}{3}";

        // 📅 Formatos de Fecha
        public const string FOLDER_DATE_FORMAT = "dd-MM-yyyy";
        public const string DEFAULT_DATE_FORMAT = "dd/MM/yyyy";
        public const string INPUT_DATE_FORMAT_YYYYMMDD = "yyyyMMdd";

        // 📄 Formatos de Presentación e Interpolación de RTF
        public const string POLICY_NUMBER_DISPLAY_FORMAT = "N° {0}";
        public const string DOCUMENT_COMBINED_FORMAT = "{0} {1}";
        public const string RTF_UNICODE_ESCAPE_FORMAT = "\\u{0}?";

        // 🔣 Caracteres Especiales y Control RTF
        public const string RTF_CHAR_AT = "@";
        public const string RTF_CHAR_BACKSLASH = "\\";
        public const string RTF_CHAR_OPEN_BRACE = "{";
        public const string RTF_CHAR_CLOSE_BRACE = "}";
        public const string RTF_ESCAPED_BACKSLASH = "\\\\";
        public const string RTF_ESCAPED_OPEN_BRACE = "\\{";
        public const string RTF_ESCAPED_CLOSE_BRACE = "\\}";

        // 🏷️ Categorías / Planes de Módulos
        public const string PLAN_MODULE_A = "A";
        public const string PLAN_MODULE_B = "B";
        public const string PLAN_MODULE_C = "C";
        public const string PLAN_MODULE_DEFAULT_D = "D";

        // 🔑 Reglas y Prefijos de Claves de Encriptación
        public const int ENCRYPTION_KEY_MAX_LENGTH = 12;
        public const string ENCRYPTION_KEY_PREFIX_DNI = "020";
        public const string ENCRYPTION_KEY_PREFIX_RUC = "010";
        public const int ENCRYPTION_KEY_DNI_SUBSTRING_START = 6;
        public const int ENCRYPTION_KEY_DNI_SUBSTRING_LENGTH = 8;
        public const int ENCRYPTION_KEY_RUC_SUBSTRING_START = 3;
        public const int ENCRYPTION_KEY_RUC_SUBSTRING_LENGTH = 11;

        // 💾 Rutas NAS (Remotas)
        public const string DB_PARAM_NAS_RUTA_APR = "NAS_RUTA_APR";
        public const string DB_PARAM_NAS_RUTA_PRIORIDAD = "NAS_RUTA_PRIORIDAD";
        public const string DB_PARAM_NAS_RUTA_NOE = "NAS_RUTA_NOE";

        // ⚙️ Configuraciones sobre gestión de archivos
        public const int SMB_READ_CHUNK_SIZE = 1024 * 64;

        // 🛠️ Separadores de Directorios Remotos (SMB / Red)
        public const char PATH_SEPARATOR_BACKSLASH = '\\';
        public const char PATH_SEPARATOR_SLASH = '/';
        public const string DELIMITER_SEMICOLON = ";";

        // 🌐 Parámetros Internos del Motor de Renderizado PDF
        public const string PUPPETEER_CHROMIUM_DIRECTORY = "Chronium";
        public const string PUPPETEER_CHROMIUM_EXECUTABLE = "chrome.exe";
        public const string PUPPETEER_ARG_NO_SANDBOX = "--no-sandbox";
        public const string PUPPETEER_ARG_DISABLE_SETUID_SANDBOX = "--disable-setuid-sandbox";
        public const string PUPPETEER_MARGIN_DEFAULT = "20px";
        public const string PUPPETEER_HTML_WRAPPER_TEMPLATE = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"></head><body>{0}</body></html>";

        // 🧼 Respuestas y Configuración SOAP (Legacy)
        public const string SOAP_DEFAULT_NAMESPACE = "http://tempuri.org/";
        public const string SOAP_ACTION_CARGA_DOCUMENTO = "http://tempuri.org/IWsGenerarPDFexterno/CargaDocumentoExterno";
        public const string SOAP_RESPONSE_STATUS_OK = "OK";
        public const string SOAP_RESPONSE_STATUS_ERROR = "ERROR";

        // 🧼 Respuesta Rest
        public const string API_SUCCESS_RESPONSE_MESSAGE = "Procesamiento terminado con éxito.";
    }
}