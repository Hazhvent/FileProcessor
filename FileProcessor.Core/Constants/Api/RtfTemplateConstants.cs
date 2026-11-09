namespace FileProcessor.Core.Constants.Api
{
    public static class RtfTemplateConstants
    {
        // 🏷️ Prefijo y valores especiales de reemplazo
        public const string TAG_PREFIX = "@";
        public const string PERCENTAGE_FULL = "100%";
        public const string SECTION_PRINCIPAL = "SECCION_PRINCIPAL";

        // 📄 Marcadores de posición estándar (Placeholders / Tags RTF - Flujo 2)
        public const string TAG_NUM_POL = "NumPol";
        public const string TAG_CERT = "Cert";
        public const string TAG_FECHA_EMISION = "FechaEmision";
        public const string TAG_FECHA_VIGENCIA = "FechaVigencia";
        public const string TAG_FECHA_FIN_VIGENCIA = "FechaFinVigencia";
        public const string TAG_NOMBRE_ASEG = "NombreAseg";
        public const string TAG_DOC_ASEG = "DocAseg";
        public const string TAG_EMAIL_ASEG = "EmailAseg";
        public const string TAG_FEC_NAC_ASEG = "FecNacAseg";
        public const string TAG_EDAD_ASEG = "EdadAseg";
        public const string TAG_PLAN_CERT = "PlanCert";
        public const string TAG_DIRECCION_ASEG = "DireccionAseg";
        public const string TAG_SEXO_ASEG = "SexoAseg";
        public const string TAG_NOMBRE_BEN = "NombreBen";
        public const string TAG_DOC_BEN = "DocBen";
        public const string TAG_RELACION_BEN_ASEG = "RelacionBenAseg";
        public const string TAG_TELEFONO_ASEG = "TelefonoAseg";
        public const string TAG_PRC = "Prc";

        // 🔍 Banderas de Tipo Documento y Clientes
        public const string TAG_ES_DNI_ASEG = "EsDniAseg";
        public const string TAG_ES_DNI_BEN = "EsDniBen";
        public const string TAG_SCLIENT = "SClient";
        public const string TAG_SCLIENT_BEN = "SClientBen";

        // 🏢 Marcadores de posición para Tabla Intermedia (Caja Los Andes - Flujo 3)
        public const string TAG_NOMBRE_BEN1 = "NombreBen1";
        public const string TAG_DIRECCION_BEN1 = "DireccionBen1";
        public const string TAG_RELACION_BEN1 = "RelacionBen1";
        public const string TAG_PORC_BEN1 = "PorcBen1";
        public const string TAG_NOMBRE_BEN2 = "NombreBen2";
        public const string TAG_DIRECCION_BEN2 = "DireccionBen2";
        public const string TAG_RELACION_BEN2 = "RelacionBen2";
        public const string TAG_PORC_BEN2 = "PorcBen2";
        public const string TAG_PLAN_CERTIFICADO = "PlanCertificado";
    }
}