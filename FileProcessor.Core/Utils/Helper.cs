using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Models.Api;
using System.Text;
using static FileProcessor.Core.Exceptions.Exceptions;

namespace FileProcessor.Core.Utils
{
    public static class Helper
    {
        private static readonly List<Secciones> DefaultSeccionesCampoFe =
        [
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_NUM_POL },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_CERT },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_FECHA_EMISION },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_FECHA_VIGENCIA },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_FECHA_FIN_VIGENCIA },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_NOMBRE_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_DOC_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_EMAIL_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_FEC_NAC_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_EDAD_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_PLAN_CERT },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_DIRECCION_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_SEXO_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_NOMBRE_BEN },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_DOC_BEN },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_RELACION_BEN_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_TELEFONO_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_PRC }
        ];

        private static readonly List<Secciones> DefaultSeccionesCajaLosAndes =
        [
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_NUM_POL },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_CERT },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_FECHA_EMISION },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_NOMBRE_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_DOC_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_SCLIENT },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_DIRECCION_ASEG },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_NOMBRE_BEN1 },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_DIRECCION_BEN1 },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_RELACION_BEN1 },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_PORC_BEN1 },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_NOMBRE_BEN2 },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_DIRECCION_BEN2 },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_RELACION_BEN2 },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_PORC_BEN2 },
            new Secciones { NombreSeccion = RtfTemplateConstants.TAG_PLAN_CERTIFICADO }
        ];

        /// <summary>
        /// Busca el parámetro por clave en la lista proporcionada o lanza una excepción si no existe.
        /// </summary>
        public static string GetRouteOrThrow(IEnumerable<Parametros> paramsList, string clave)
        {
            return paramsList?.FirstOrDefault(p => p.Clave == clave)?.Parametro
                ?? throw new NotFoundException(string.Format(ErrorConstants.INTERNAL_NOT_FOUND_DB_PARAMETER, clave));
        }

        /// <summary>
        /// Descompone una ruta UNC en componentes SMB: Host, ShareFolder y RelativePath.
        /// </summary>
        public static ServerPath? ParseServerPath(string? uncPath)
        {
            if (string.IsNullOrWhiteSpace(uncPath)) return null;

            char[] pathSeparators = [SettingsConstants.PATH_SEPARATOR_BACKSLASH, SettingsConstants.PATH_SEPARATOR_SLASH];
            string cleanPath = uncPath.Trim().TrimStart(pathSeparators);

            string[] parts = cleanPath.Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2) return null;

            string host = parts[0];
            string shareFolder = parts[1];
            string relativePath = parts.Length > 2
                ? string.Join(SettingsConstants.PATH_SEPARATOR_BACKSLASH, parts.Skip(2))
                : string.Empty;

            return new ServerPath
            {
                Host = host,
                ShareFolder = shareFolder,
                RelativePath = relativePath
            };
        }

        // 📝 Para campos de TEXTO en Oracle que requieren el placeholder legacy
        public static string SanitizeText(string? value, string fallbackPlaceholder = ValidationConstants.LEGACY_EMPTY_PLACEHOLDER)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Trim() != fallbackPlaceholder
                ? value.Trim()
                : fallbackPlaceholder;
        }

        // 📅 Para campos FECHA en Oracle (convierte '-' y vacíos a null real)
        public static string? SanitizeDate(string? value, string fallbackPlaceholder = ValidationConstants.LEGACY_EMPTY_PLACEHOLDER)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            string trimmed = value.Trim();
            return trimmed == fallbackPlaceholder ? null : trimmed;
        }

        public static decimal ConvertBytesToMegabytes(long bytes)
        {
            if (bytes <= 0) return 0.0m;
            return bytes / ValidationConstants.BYTES_TO_MB_CONVERSION_FACTOR;
        }

        public static string ExtractInternalPath(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return string.Empty;

            char[] pathSeparators = [SettingsConstants.PATH_SEPARATOR_BACKSLASH, SettingsConstants.PATH_SEPARATOR_SLASH];

            return relativePath
                .Trim(pathSeparators)
                .Replace(SettingsConstants.PATH_SEPARATOR_SLASH, SettingsConstants.PATH_SEPARATOR_BACKSLASH);
        }

        public static int CalculateNextChunkSize(long fileSize, long bytesRead, int maxChunkSize)
        {
            if (bytesRead >= fileSize) return 0;

            return (int)Math.Min(maxChunkSize, fileSize - bytesRead);
        }

        /// <summary>
        /// Codifica adecuadamente un texto a la sintaxis RTF preservando caracteres ASCII estándar y escapando caracteres de control.
        /// </summary>
        public static string CodificarStringRtf(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            StringBuilder sb = new();
            foreach (char c in input)
            {
                int intCode = Convert.ToInt32(c);

                if (c == SettingsConstants.PATH_SEPARATOR_BACKSLASH)
                    sb.Append(SettingsConstants.RTF_ESCAPED_BACKSLASH);
                else if (c == SettingsConstants.RTF_CHAR_OPEN_BRACE[0])
                    sb.Append(SettingsConstants.RTF_ESCAPED_OPEN_BRACE);
                else if (c == SettingsConstants.RTF_CHAR_CLOSE_BRACE[0])
                    sb.Append(SettingsConstants.RTF_ESCAPED_CLOSE_BRACE);
                else if (c == SettingsConstants.RTF_CHAR_AT[0])
                    sb.Append(SettingsConstants.RTF_CHAR_AT);
                else if (intCode >= 32 && intCode <= 126)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(string.Format(SettingsConstants.RTF_UNICODE_ESCAPE_FORMAT, intCode));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reemplaza las etiquetas de la plantilla RTF con los datos de un certificado estándar (CertificadoCampoFe).
        /// </summary>
        public static string ProcessRtfTemplateCampoFe(string rtfContent, List<Secciones> secciones, CertificadoCampoFe cert)
        {
            if (string.IsNullOrWhiteSpace(rtfContent) || cert == null)
                return rtfContent ?? string.Empty;

            if (secciones == null || secciones.Count == 0 || secciones.Any(s => string.Equals(s.NombreSeccion?.Trim(), RtfTemplateConstants.SECTION_PRINCIPAL, StringComparison.OrdinalIgnoreCase)))
            {
                secciones = DefaultSeccionesCampoFe;
            }

            foreach (var sec in secciones)
            {
                string nombreSeccionLimpio = sec.NombreSeccion?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(nombreSeccionLimpio)) continue;

                nombreSeccionLimpio = nombreSeccionLimpio
                    .Replace(SettingsConstants.RTF_CHAR_AT, string.Empty)
                    .Replace(SettingsConstants.RTF_CHAR_OPEN_BRACE, string.Empty)
                    .Replace(SettingsConstants.RTF_CHAR_CLOSE_BRACE, string.Empty)
                    .Replace(SettingsConstants.RTF_CHAR_BACKSLASH, string.Empty)
                    .Trim();

                if (string.IsNullOrEmpty(nombreSeccionLimpio)) continue;

                string tag = RtfTemplateConstants.TAG_PREFIX + nombreSeccionLimpio;
                string escapedTag = SettingsConstants.RTF_CHAR_BACKSLASH + tag;
                string doubleEscapedTag = SettingsConstants.RTF_ESCAPED_BACKSLASH + tag;

                if (!rtfContent.Contains(tag, StringComparison.OrdinalIgnoreCase) &&
                    !rtfContent.Contains(escapedTag, StringComparison.OrdinalIgnoreCase) &&
                    !rtfContent.Contains(doubleEscapedTag, StringComparison.OrdinalIgnoreCase))
                    continue;

                string valor = string.Empty;

                if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_NUM_POL, StringComparison.OrdinalIgnoreCase))
                    valor = string.Format(SettingsConstants.POLICY_NUMBER_DISPLAY_FORMAT, cert.NumeroPoliza);
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_CERT, StringComparison.OrdinalIgnoreCase))
                    valor = cert.Certificado.ToString();
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_FECHA_EMISION, StringComparison.OrdinalIgnoreCase))
                    valor = cert.FechaEmision != default ? cert.FechaEmision.ToString(SettingsConstants.DEFAULT_DATE_FORMAT) : string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_FECHA_VIGENCIA, StringComparison.OrdinalIgnoreCase))
                    valor = cert.FechaVigencia != default ? cert.FechaVigencia.ToString(SettingsConstants.DEFAULT_DATE_FORMAT) : string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_FECHA_FIN_VIGENCIA, StringComparison.OrdinalIgnoreCase))
                    valor = cert.FechaFinVigencia != default ? cert.FechaFinVigencia.ToString(SettingsConstants.DEFAULT_DATE_FORMAT) : string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_NOMBRE_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.NombreAsegurado ?? string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_DOC_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = string.Format(SettingsConstants.DOCUMENT_COMBINED_FORMAT, cert.TipoDocAsegurado, cert.DocumentoAsegurado).Trim();
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_EMAIL_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.EmailAsegurado ?? string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_FEC_NAC_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.FechaNacimientoAsegurado != default ? cert.FechaNacimientoAsegurado.ToString(SettingsConstants.DEFAULT_DATE_FORMAT) : string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_EDAD_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.EdadAsegurado.ToString();
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_PLAN_CERT, StringComparison.OrdinalIgnoreCase))
                    valor = cert.NModulec switch
                    {
                        1 => SettingsConstants.PLAN_MODULE_A,
                        2 => SettingsConstants.PLAN_MODULE_B,
                        3 => SettingsConstants.PLAN_MODULE_C,
                        _ => SettingsConstants.PLAN_MODULE_DEFAULT_D
                    };
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_DIRECCION_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.DireccionAsegurado ?? string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_SEXO_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.SexoAsegurado ?? string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_NOMBRE_BEN, StringComparison.OrdinalIgnoreCase))
                    valor = cert.NombreBeneficiario ?? string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_DOC_BEN, StringComparison.OrdinalIgnoreCase))
                    valor = string.Format(SettingsConstants.DOCUMENT_COMBINED_FORMAT, cert.TipoDocBeneficiario, cert.DocumentoBeneficiario).Trim();
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_RELACION_BEN_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.RelacionBeneficiario ?? string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_TELEFONO_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.TelefonoAsegurado ?? string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_PRC, StringComparison.OrdinalIgnoreCase))
                    valor = !string.IsNullOrWhiteSpace(cert.NombreBeneficiario) ? RtfTemplateConstants.PERCENTAGE_FULL : string.Empty;

                string valorCodificado = CodificarStringRtf(valor);

                rtfContent = rtfContent
                    .Replace(doubleEscapedTag, valorCodificado, StringComparison.OrdinalIgnoreCase)
                    .Replace(escapedTag, valorCodificado, StringComparison.OrdinalIgnoreCase)
                    .Replace(tag, valorCodificado, StringComparison.OrdinalIgnoreCase);
            }

            return rtfContent;
        }

        /// <summary>
        /// Reemplaza las etiquetas de la plantilla RTF con los datos de tabla intermedia (CertificadoCajaLosAndes).
        /// </summary>
        public static string ProcessRtfTemplateCajaLosAndes(string rtfContent, List<Secciones> secciones, CertificadoCajaLosAndes cert)
        {
            if (string.IsNullOrWhiteSpace(rtfContent) || cert == null)
                return rtfContent ?? string.Empty;

            if (secciones == null || secciones.Count == 0 || secciones.Any(s => string.Equals(s.NombreSeccion?.Trim(), RtfTemplateConstants.SECTION_PRINCIPAL, StringComparison.OrdinalIgnoreCase)))
            {
                secciones = DefaultSeccionesCajaLosAndes;
            }

            foreach (var sec in secciones)
            {
                string nombreSeccionLimpio = sec.NombreSeccion?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(nombreSeccionLimpio)) continue;

                nombreSeccionLimpio = nombreSeccionLimpio
                    .Replace(SettingsConstants.RTF_CHAR_AT, string.Empty)
                    .Replace(SettingsConstants.RTF_CHAR_OPEN_BRACE, string.Empty)
                    .Replace(SettingsConstants.RTF_CHAR_CLOSE_BRACE, string.Empty)
                    .Replace(SettingsConstants.RTF_CHAR_BACKSLASH, string.Empty)
                    .Trim();

                if (string.IsNullOrEmpty(nombreSeccionLimpio)) continue;

                string tag = RtfTemplateConstants.TAG_PREFIX + nombreSeccionLimpio;
                string escapedTag = SettingsConstants.RTF_CHAR_BACKSLASH + tag;
                string doubleEscapedTag = SettingsConstants.RTF_ESCAPED_BACKSLASH + tag;

                if (!rtfContent.Contains(tag, StringComparison.OrdinalIgnoreCase) &&
                    !rtfContent.Contains(escapedTag, StringComparison.OrdinalIgnoreCase) &&
                    !rtfContent.Contains(doubleEscapedTag, StringComparison.OrdinalIgnoreCase))
                    continue;

                string valor = string.Empty;

                if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_NUM_POL, StringComparison.OrdinalIgnoreCase))
                    valor = string.Format(SettingsConstants.POLICY_NUMBER_DISPLAY_FORMAT, cert.NumeroPoliza);
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_CERT, StringComparison.OrdinalIgnoreCase))
                    valor = cert.Certificado.ToString();
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_FECHA_EMISION, StringComparison.OrdinalIgnoreCase))
                    valor = cert.FechaEmision != default ? cert.FechaEmision.ToString(SettingsConstants.DEFAULT_DATE_FORMAT) : string.Empty;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_NOMBRE_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.NombreAsegurado;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_DOC_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.DocumentoAsegurado;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_SCLIENT, StringComparison.OrdinalIgnoreCase))
                    valor = cert.SClientAsegurado;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_DIRECCION_ASEG, StringComparison.OrdinalIgnoreCase))
                    valor = cert.DireccionAsegurado;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_NOMBRE_BEN1, StringComparison.OrdinalIgnoreCase))
                    valor = cert.NombreBeneficiario1;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_DIRECCION_BEN1, StringComparison.OrdinalIgnoreCase))
                    valor = cert.DireccionBeneficiario1;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_RELACION_BEN1, StringComparison.OrdinalIgnoreCase))
                    valor = cert.RelacionBeneficiario1;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_PORC_BEN1, StringComparison.OrdinalIgnoreCase))
                    valor = cert.PorcentajeBeneficiario1;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_NOMBRE_BEN2, StringComparison.OrdinalIgnoreCase))
                    valor = cert.NombreBeneficiario2;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_DIRECCION_BEN2, StringComparison.OrdinalIgnoreCase))
                    valor = cert.DireccionBeneficiario2;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_RELACION_BEN2, StringComparison.OrdinalIgnoreCase))
                    valor = cert.RelacionBeneficiario2;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_PORC_BEN2, StringComparison.OrdinalIgnoreCase))
                    valor = cert.PorcentajeBeneficiario2;
                else if (string.Equals(nombreSeccionLimpio, RtfTemplateConstants.TAG_PLAN_CERTIFICADO, StringComparison.OrdinalIgnoreCase))
                    valor = cert.PlanCertificado;

                string valorCodificado = CodificarStringRtf(valor);

                rtfContent = rtfContent
                    .Replace(doubleEscapedTag, valorCodificado, StringComparison.OrdinalIgnoreCase)
                    .Replace(escapedTag, valorCodificado, StringComparison.OrdinalIgnoreCase)
                    .Replace(tag, valorCodificado, StringComparison.OrdinalIgnoreCase);
            }

            return rtfContent;
        }

        /// <summary>
        /// Determina y formatea la clave de cifrado PDF (DNI/RUC) aplicando reglas de subcadena según longitud y prefijos.
        /// </summary>
        public static string DetermineEncryptionKey(string docContratanteRequest, string docAseguradoBD)
        {
            string dni = string.IsNullOrWhiteSpace(docContratanteRequest) ? docAseguradoBD : docContratanteRequest;
            dni = (dni ?? string.Empty).Trim();

            if (dni.Length > SettingsConstants.ENCRYPTION_KEY_MAX_LENGTH)
            {
                if ((dni.StartsWith(ValidationConstants.LEGACY_EMPTY_PLACEHOLDER) || dni.StartsWith(SettingsConstants.ENCRYPTION_KEY_PREFIX_DNI)) &&
                    dni.Length >= SettingsConstants.ENCRYPTION_KEY_DNI_SUBSTRING_START + SettingsConstants.ENCRYPTION_KEY_DNI_SUBSTRING_LENGTH)
                {
                    return dni.Substring(SettingsConstants.ENCRYPTION_KEY_DNI_SUBSTRING_START, SettingsConstants.ENCRYPTION_KEY_DNI_SUBSTRING_LENGTH);
                }
                if (dni.StartsWith(SettingsConstants.ENCRYPTION_KEY_PREFIX_RUC) &&
                    dni.Length >= SettingsConstants.ENCRYPTION_KEY_RUC_SUBSTRING_START + SettingsConstants.ENCRYPTION_KEY_RUC_SUBSTRING_LENGTH)
                {
                    return dni.Substring(SettingsConstants.ENCRYPTION_KEY_RUC_SUBSTRING_START, SettingsConstants.ENCRYPTION_KEY_RUC_SUBSTRING_LENGTH);
                }
            }

            return dni;
        }
    }
}