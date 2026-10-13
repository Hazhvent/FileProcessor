namespace FileProcessor.Core.Constants.Messages
{
    // [no tocar 🔒] 
    public static class ErrorConstants
    {
        // Errores internos
        public const string INTERNAL_NOT_FOUND_CONFIG_SECTION = "La sección '{0}' no se encontró o no pudo ser mapeada en la configuración.";
        public const string INTERNAL_SMTP_PORT_INVALID = "Error de configuración SMTP: El puerto '{0}' recuperado de la base de datos no es un número válido.";
        public const string INTERNAL_SMTP_CONFIG_EMPTY = "Error de configuración SMTP: La colección de parámetros SMTP externa no contiene elementos o es nula.";
        public const string INTERNAL_NOT_FOUND_DB_PARAMETER = "Error interno: El parámetro de configuración '{0}' no se encontró o está vacío en la base de datos.";
        public const string INTERNAL_POLICY_REGISTRATION_FAILED = "Error crítico de servidor: No se pudo registrar la póliza de control digital {0} en el sistema.";
        public const string INTERNAL_CORE_UPDATE_FAILED = "Error crítico de servidor: Falló la actualización de los datos extendidos en el Core para la póliza {0}.";
        public const string INTERNAL_PUPPETEER_EXECUTABLE_NOT_FOUND = "Error de infraestructura: No se encontró el ejecutable de Chromium en la ruta: '{0}'.";
        public const string INTERNAL_RTF_FILE_EMPTY = "Error de infraestructura: El contenido del archivo de plantilla RTF '{0}' en el almacenamiento remoto está vacío.";
        public const string INTERNAL_RTF_PROCESSING_FAILED = "Error de procesamiento: No se pudo realizar el reemplazo de etiquetas en la plantilla RTF '{0}'.";

        // 🔥 NUEVAS CONSTANTES SOAP PARA EL FORMATEO DE ERRORES 🔥
        public const string INTERNAL_SOAP_FATAL_ERROR = "Ocurrió un error interno procesando la solicitud SOAP. Revise los logs del servidor. Detalle: {0}";
        public const string SOAP_VALIDATION_ERROR_DETAIL = "{0} Detalle: {1}";
        public const string SOAP_VALIDATION_FIELD_ERROR = "[{0}]: {1}";
        public const string SOAP_VALIDATION_SEPARATOR = " | ";

        public const string INTERNAL_NOT_FOUND_SOAP_ENDPOINT_PATH = $"Falta configurar '{Api.SettingsConstants.SOAP_SETTINGS_ENDPOINT_PATH_KEY}' en el appsettings.json. Esta ruta es obligatoria para el servicio SOAP.";

        // ERRORES ESPECÍFICOS DEL PROTOCOLO SMB / NETWORK
        public const string INTERNAL_SMB_HOST_NOT_FOUND = "Fallo SMB: No se pudo resolver el nombre del host o dirección IP '{0}' en la red (DNS / Host desconocido).";
        public const string INTERNAL_SMB_TIMEOUT = "Fallo SMB: Se agotó el tiempo de espera al comunicarse con el servidor o el puerto SMB (445) no está alcanzable.";
        public const string INTERNAL_SMB_AUTH_CREDENTIALS_INVALID = "Fallo SMB: El usuario o contraseña especificados en el archivo de configuración de la API son incorrectos.";
        public const string INTERNAL_SMB_AUTH_READ_DENIED = "Fallo SMB: Acceso denegado. El usuario de red no tiene permisos de LECTURA para inspeccionar o acceder a esta ruta.";
        public const string INTERNAL_SMB_AUTH_WRITE_DENIED = "Fallo SMB: Acceso denegado. El usuario de red puede leer la ruta, pero carece de permisos de ESCRITURA/CREACIÓN para crear o modificar directorios en el destino.";
        public const string INTERNAL_SMB_FILE_SIZE_NULL = "Fallo SMB: No se pudo determinar el tamaño del archivo de origen en el almacenamiento remoto (tamaño nulo o ilegible).";
        public const string INTERNAL_PDF_ENCRYPTION_FAILED_WITH_DETAIL = "Error de infraestructura: Fallo crítico al aplicar las directivas de seguridad en el flujo del PDF. Detalle técnico: {0}";
        public const string INTERNAL_SMB_FILE_LOCKED = "Fallo SMB: El archivo está bloqueado o en uso por otro proceso (Sharing Violation).";
        public const string INTERNAL_SMB_GENERIC_ERROR = "Fallo SMB: El servidor rechazó la operación (Código: {0}).";

        // Errores de solicitud 
        public const string REQUEST_JSON_SYNTAX_INVALID = "La estructura interna de los datos no tiene un formato válido o presenta errores de sintaxis.";
        public const string REQUEST_DOCUMENTS_INCOMPLETE = "La información de los documentos es nula o incompleta.";
        public const string REQUEST_USERS_INCOMPLETE = "La información de los usuarios es nula o incompleta.";
        public const string REQUEST_DOCUMENT_NAME_REQUIRED = "El nombre del documento es obligatorio.";
        public const string REQUEST_TEMPLATE_NOT_FOUND = "No se encontró ninguna plantilla con el nombre '{0}'.";
        public const string REQUEST_DIRECTORY_INCOMPLETE = "Falta especificar el directorio de almacenamiento.";
        public const string REQUEST_UNC_PATH_INVALID = "La ruta de directorio '{0}' especificada no tiene un formato UNC válido (ejemplo: \\\\Servidor\\CarpetaCompartida).";
        public const string REQUEST_PATH_NOT_EXISTS = "Fallo SMB: La ruta o directorio '{0}' solicitado no existe en el servidor.";
        public const string REQUEST_RAMO_INVALID = "El Ramo especificado debe ser un valor numérico mayor a cero.";
        public const string REQUEST_PRODUCTO_INVALID = "El Producto especificado debe tener algun valor numérico.";
        public const string REQUEST_POLICY_REQUIRED = "El número de Póliza especificado debe ser un valor numérico mayor a cero.";
        public const string REQUEST_CERTIFICATE_REQUIRED = "El número de Certificado especificado debe ser un valor numérico mayor a cero.";
        public const string REQUEST_TEMPLATE_NAME_REQUIRED = "El nombre de la plantilla del documento es obligatorio.";
        public const string REQUEST_CONTRACTOR_DOC_REQUIRED = "El documento del contratante es obligatorio.";
        public const string REQUEST_USER_NAME_REQUIRED = "El nombre del usuario es obligatorio.";
        public const string REQUEST_USER_EMAIL_INVALID = "El correo del usuario es obligatorio y debe tener un formato válido.";
        public const string REQUEST_USER_DNI_REQUIRED = "El DNI del usuario es obligatorio para este tipo de solicitud.";
        public const string REQUEST_GREETING_INVALID = $"El campo GeneraSaludo solo acepta los valores '{Api.ValidationConstants.GENERA_SALUDO_SI}' o '{Api.ValidationConstants.GENERA_SALUDO_NO}'.";

        // Errores de lógica de negocio
        public const string REQUEST_POLICY_NOT_FOUND = "Error de negocio: La póliza {0} no existe en el sistema.";
        public const string REQUEST_CERTIFICATE_NOT_FOUND = "Error de negocio: El certificado {0} no corresponde a la póliza {1}.";
        public const string REQUEST_CERTIFICATE_RTF_DATA_NOT_FOUND = "No se encontró información para generar el RTF del certificado {0}.";
        public const string REQUEST_TEMPLATE_SECTIONS_NOT_FOUND = "No se encontraron secciones ni etiquetas configuradas en la base de datos para el documento con ID {0}.";
        public const string REQUEST_CURRENCY_NOT_FOUND = "Error de negocio: No se encontró la moneda configurada para la póliza {0}.";
        public const string REQUEST_APPROVAL_RULE_NOT_FOUND = "No se encontraron reglas de aprobación configuradas para el monto {0} en la moneda {1}.";
        public const string REQUEST_REVIEWERS_NOT_FOUND = "La regla de aprobación está incompleta: No hay revisores asignados para el monto {0} en la moneda {1}.";
        public const string REQUEST_DESTINATARIO_NOT_FOUND = "Validación: No se encuentra el destinatario con DNI {0}.";
        public const string REQUEST_BUSINESS_PRODUCT_MISMATCH = "El producto de la póliza no coincide con los productos permitidos para el documento.";
        public const string REQUEST_BUSINESS_BRANCH_MISMATCH = "El ramo de la póliza no coincide con el ramo del documento.";
        public const string REQUEST_CONTRACTOR_NOT_FOUND = "Error de negocio: No se encontró el contratante para la póliza {0}.";
        public const string REQUEST_INSURED_ROLE_NOT_FOUND = "Error de negocio: No se encontró el rol del Asegurado para la póliza {0}.";
        public const string REQUEST_PREMIUMS_NOT_FOUND = "Error de negocio: No se encontró la prima configurada para la póliza {0}.";
        public const string REQUEST_ADMIN_USER_NOT_FOUND = "Error de negocio: No se encontró el usuario externo de sistema ({0}) en la base de datos.";
        public const string REQUEST_INSURED_NOT_FOUND = "Error de negocio: No se encontraron los datos maestros del Asegurado para la póliza {0}.";
        public const string REQUEST_ENCRYPTION_KEY_NOT_FOUND = "Error de negocio: No se pudo obtener la clave de encriptación PDF para el cliente {0}.";
        public const string REQUEST_IMPORTED_FILE_NOT_FOUND = "El archivo importado '{0}' no se encuentra en la ruta del NAS '{1}'.";

        // Propiedades para mapeo de errores (ApiSubError / UnprocessableEntity)
        public const string PROPERTY_PRODUCTO = "Producto";
        public const string PROPERTY_RAMO = "Ramo";
    }
}