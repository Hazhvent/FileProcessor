namespace FileProcessor.Core.Constants.Messages
{
    public static class LogsConstants
    {
        // ⚙️ PARÁMETROS DE CONFIGURACIÓN DE LOGS (SERILOG)
        public const string LOGGER_FILE_PATH_KEY = $"{Api.SettingsConstants.APPLICATION_SETTINGS_SECTION_NAME}:LogDirectoryPath";
        public const string LOGGER_CATEGORY_MICROSOFT = "Microsoft";
        public const string LOGGER_CATEGORY_MICROSOFT_HOSTING_LIFETIME = "Microsoft.Hosting.Lifetime";
        public const int LOGGER_FILE_RETAINED_COUNT_LIMIT = 30;
        public const string LOGGER_CONSOLE_OUTPUT_TEMPLATE = "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
        public const string LOGGER_FILE_OUTPUT_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        // 💬 VALORES DE PLANTILLA PARA LOGS
        public const string LOG_VALUE_NOT_AVAILABLE = "N/A";
        public const string LOG_VALUE_YES = "SÍ";
        public const string LOG_VALUE_NO = "NO";

        // 🟢 INFO LOGS
        public const string INFO_BATCH_STARTED = "Iniciando procesamiento de lote con {Count} documentos. Ruta Origen: {DirectoryPath}";
        public const string INFO_TEMPLATES_VALIDATED = "Validación inicial de {Count} plantillas completada en caché.";
        public const string INFO_DIRECTORIES_PREPARED = "Directorios de destino locales y NAS preparados correctamente.";
        public const string INFO_LOTE_GENERATED = "Nuevo Lote de procesamiento asignado: {Lote}";
        public const string INFO_PROCESSING_DOCUMENT = "Procesando documento '{DocumentName}' [Póliza: {Policy} | Certificado: {Certificate}]";
        public const string INFO_FILE_READ_NAS = "Archivo origen leído exitosamente desde el NAS ({Size} bytes).";
        public const string INFO_PDF_ENCRYPTED = "Documento PDF '{DocumentName}' cifrado con éxito.";
        public const string INFO_RTF_CONVERTED = "Plantilla RTF interpolada y convertida a PDF: '{DocumentName}'.";
        public const string INFO_DOCUMENT_SAVED_NAS = "Documento '{DocumentName}' escrito correctamente en el NAS [{NasPath}].";
        public const string INFO_DATABASE_REGISTERED = "Documento '{DocumentName}' registrado en la BD (Requiere Aprobación: {RequiereAprobacion}).";
        public const string INFO_EMAILS_SENT = "Notificaciones enviadas a {Count} correos de revisores.";
        public const string INFO_USERS_REGISTERED = "Usuarios del lote {Lote} registrados exitosamente en la BD.";
        public const string INFO_BATCH_FINISHED = "Procesamiento del lote {Lote} finalizado con éxito.";

        // 🔀 IDENTIFICADORES DE FLUJOS
        public const string FLOW_EXTERNAL_FILE = "Flujo 'Archivo externo' detectado, iniciando proceso.";
        public const string FLOW_CAMPO_FE = "Flujo 'CampoFe' detectado, iniciando proceso.";
        public const string FLOW_CAJA_LOS_ANDES = "Flujo 'CajaLosAndes' detectado, iniciando proceso.";

        // 🟠 WARN LOGS
        public const string WARN_FILE_REJECTED_SIZE = "El archivo '{DocumentName}' excede el tamaño máximo permitido. Movido a carpeta de Rechazados.";

        // 🔴 ERROR LOGS
        public const string ERROR_FILE_PROCESSING = "Hubo un error en el procesamiento.";
    }
}