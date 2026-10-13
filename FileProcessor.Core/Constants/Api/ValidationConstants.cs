namespace FileProcessor.Core.Constants.Api
{
    public static class ValidationConstants
    {
        public const string LEGACY_EMPTY_PLACEHOLDER = "-";
        public const string TEMPLATE_CAMPO_FE = "Solicitud Certificado Clausulas Campo Fe";
        public const decimal MAX_FILE_SIZE_MB = 30.0m;
        public const decimal BYTES_TO_MB_CONVERSION_FACTOR = 1024.0m * 1024.0m;
        public const string GENERA_SALUDO_SI = "S";
        public const string GENERA_SALUDO_NO = "N";
        public const string GENDER_MALE = "Hombre";
        public const string GENDER_FEMALE = "Mujer";
        public const string GREETING_FORMAT_MALE = "Estimado Sr. {0}";
        public const string GREETING_FORMAT_FEMALE = "Estimada Sra. {0}";
        public const string FULL_NAME_FORMAT = "{0} {1}";
        public const string PROPERTY_DNI = "DNI";
    }
}