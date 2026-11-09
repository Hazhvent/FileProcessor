using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Database;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Interfaces.Repositories;
using System.Data;
using System.Globalization;

namespace FileProcessor.Infrastructure.Persistence.Repositories
{
    public class Repository(IDbConnection dbConnection) : IRepository
    {
        private readonly IDbConnection _dbConnection = dbConnection;

        public async Task<Policy?> GetPolizaAsync(long idPoliza)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_POLIZA, idPoliza }
            };

            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Policy>(
                ProceduresConstants.PD_SP_GETPOLIZAS,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<IEnumerable<Secciones>> GetSeccionesByIdAsync(int idDocumento)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.PS_ID_DOCUMENTO, idDocumento }
            };

            return await _dbConnection.QueryOracleAsync<Secciones>(
                ProceduresConstants.PD_SP_GETSECCIONESBYID,
                parametros,
                ParametersConstants.CURSOR_SEC
            );
        }

        public async Task<Policy?> GetCertificadoAsync(long poliza, long idCertificado)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_POLIZA, poliza },
                { ParametersConstants.ID_CERTIFICADO, idCertificado }
            };

            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Policy>(
                ProceduresConstants.SP_GETCERTIFICADO,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<Client?> GetDataClientAsync(string sclient)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.P_SCLIENTE, sclient }
            };

            // 1. Evadimos el mapeo estricto con dynamic
            var row = await _dbConnection.QueryOracleFirstOrDefaultAsync<dynamic>(
                ProceduresConstants.PD_SP_GETCLIENTE,
                parametros,
                ParametersConstants.CURSOR_OP
            );

            if (row == null) return null;

            // 2. Dapper nos da un ExpandoObject que tratamos como diccionario
            var dict = (IDictionary<string, object>)row;
            var valores = dict.Values.ToList();

            // 3. Extraemos por posición física ignorando los nombres satánicos de Oracle
            string codigoCliente = valores.ElementAtOrDefault(0)?.ToString() ?? string.Empty; // Primera columna
            string nombreCliente = valores.ElementAtOrDefault(1)?.ToString() ?? string.Empty; // Segunda columna

            // 4. Devolvemos el DTO mapeado a mano de forma segura
            return new Client
            {
                SCient = codigoCliente,
                SClieName = nombreCliente
            };
        }

        public async Task<Client?> GetDataDestinatarioAsync(string dni)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.P_DNI, dni.Trim() }
            };

            // 1. En lugar de <Client>, pedimos <dynamic> para evadir el mapeo estricto
            var row = await _dbConnection.QueryOracleFirstOrDefaultAsync<dynamic>(
                ProceduresConstants.SP_GETDESTINATARIO,
                parametros,
                ParametersConstants.CURSOR_OP
            );

            if (row == null) return null;

            // 2. Dapper devuelve un ExpandoObject que podemos tratar como diccionario
            var dict = (IDictionary<string, object>)row;

            // 3. ¡Agarramos el primer valor sin importar cómo diablos se llame la columna!
            string sexoEncontrado = dict.Values.FirstOrDefault()?.ToString() ?? string.Empty;

            // 4. Devolvemos tu objeto Client limpio y mapeado a mano
            return new Client
            {
                SSEXCLIEN = sexoEncontrado
            };
        }

        public async Task<Aprobacion?> SetAprobacionAsync(long idPoliza, int idUsuario, int estatus, string nombreArchivo, int idGenerico, int idUsuarioCreador, int asignada, int emitida)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_POL, idPoliza },
                { ParametersConstants.ID_US, idUsuario },
                { ParametersConstants.ESTATUS, estatus },
                { ParametersConstants.NOMBRE_ARCH, nombreArchivo },
                { ParametersConstants.ID_GENERICO, idGenerico },
                { ParametersConstants.ID_US_CREADOR, idUsuarioCreador },
                { ParametersConstants.ASIG, asignada },
                { ParametersConstants.EMITIDA_VAL, emitida }
            };

            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Aprobacion>(
                ProceduresConstants.SP_POLIZAALTA,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<Aprobacion?> SetSinAprobacionAsync(long idPoliza, int idUsuario, int estatus, string nombreArchivo, int idGenerico, int idUsuarioCreador, int emitida)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_POL, idPoliza },
                { ParametersConstants.ID_US, idUsuario },
                { ParametersConstants.ESTATUS, estatus },
                { ParametersConstants.NOMBRE_ARCH, nombreArchivo },
                { ParametersConstants.ID_GENERICO, idGenerico },
                { ParametersConstants.ID_US_CREADOR, idUsuarioCreador },
                { ParametersConstants.ASIG, 1 },
                { ParametersConstants.EMITIDA_VAL, emitida }
            };

            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Aprobacion>(
                ProceduresConstants.SP_SINAPROBPOLIZAALTA,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<Bitacora?> SetBitacoraAsync(long idPoliza, string ruta, int estatus, string nombreArchivo, int idGenerico, int idUsuarioCreador, int idUsuario)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_POLIZA, idPoliza },
                { ParametersConstants.RUTA, ruta },
                { ParametersConstants.NOMBRE_ARCHIVO, nombreArchivo },
                { ParametersConstants.ESTATUS, estatus },
                { ParametersConstants.ID_GENERICO, idGenerico },
                { ParametersConstants.ID_USUARIO_CREADOR, idUsuarioCreador },
                { ParametersConstants.ID_USUARIO, idUsuario }
            };

            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Bitacora>(
                ProceduresConstants.SP_BITACORAALTA,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<Polizas?> SetPolizaAsync(
            long idPoliza, int firma, int ramo, double suma, int producto, DateTime fecha,
            string contratante, string asegurado, string lote, int idTipoDocumento,
            int idRegla, int idDocumento, long idcertificado, string psGeneraSaludo)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_POLIZA, idPoliza },
                { ParametersConstants.FIRMA, firma },
                { ParametersConstants.RAMO, ramo },
                { ParametersConstants.SUMA, suma },
                { ParametersConstants.PRODUCTO, producto },
                { ParametersConstants.FECHA, fecha },
                { ParametersConstants.CONTRATANTE, contratante },
                { ParametersConstants.ASEGURADO, asegurado },
                { ParametersConstants.LOTE, lote },
                { ParametersConstants.ID_TIPO_DOC, idTipoDocumento },
                { ParametersConstants.ID_REGLA, idRegla },
                { ParametersConstants.ID_RTF, idDocumento },
                { ParametersConstants.PD_CERTIFICADO, idcertificado },
                { ParametersConstants.PS_GENERA_SALUDO, psGeneraSaludo }
            };

            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Polizas>(
                ProceduresConstants.SP_POLIZA_EXTERNO,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<bool> UpdateAsync(
           string idcliente, int primacertificado, string polifecemision, string polifecvig,
           string monedaCorto, string ruc, string dir, string nom, string sex,
           string correo, string tel, string nac, string dir2, int primaPoliza,
           string certfecem, string certfecvig, int idtipodoc, int idgen, int idMoneda)
        {
            var parametros = new Dictionary<string, object?>
            {
                { ParametersConstants.ID_CLIENTE, idcliente },
                { ParametersConstants.PRIMA_CERTIFICADO, primacertificado },
                { ParametersConstants.POLI_FEC_EMISION, polifecemision },
                { ParametersConstants.POLI_FEC_VIG, polifecvig },
                { ParametersConstants.MONEDA_CORTO, monedaCorto },
                { ParametersConstants.RUC, ruc },
                { ParametersConstants.DIR, dir },
                { ParametersConstants.P_NOMBRES, nom },
                { ParametersConstants.SEX, sex },
                { ParametersConstants.CORREO, correo },
                { ParametersConstants.TEL, tel },
                { ParametersConstants.NAC, nac },
                { ParametersConstants.DIR2, dir2 },
                { ParametersConstants.PRIMA_POLIZA, primaPoliza },
                { ParametersConstants.CERT_FEC_EM, certfecem == ValidationConstants.LEGACY_EMPTY_PLACEHOLDER ? null : certfecem },
                { ParametersConstants.CERT_FEC_VIG, certfecvig == ValidationConstants.LEGACY_EMPTY_PLACEHOLDER ? null : certfecvig },
                { ParametersConstants.P_ID_TIPO_DOC_UP, idtipodoc },
                { ParametersConstants.P_ID_GENERICO, idgen },
                { ParametersConstants.P_ID_MONEDA_UP, idMoneda }
            };

            await _dbConnection.QueryOracleFirstOrDefaultAsync<object>(
                ProceduresConstants.SP_ActPoliza,
                parametros!,
                ParametersConstants.CURSOR_CO
            );

            return true;
        }

        public async Task<IEnumerable<Address>> GetAddressAsync(string cliente)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.CLIENTE, cliente }
            };
            return await _dbConnection.QueryOracleAsync<Address>(
                ProceduresConstants.SP_GETADDRESS,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<ParametrosCargaDocumentoExterno?> GetDatosDocAsync(string nom)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.NOM, nom }
            };
            return await _dbConnection.QueryOracleFirstOrDefaultAsync<ParametrosCargaDocumentoExterno>(
                ProceduresConstants.SP_GETDATOSDOC,
                parametros,
                ParametersConstants.CURSOR_CO
            );
        }

        public async Task<Moneda?> GetMonedaAsync(long poliza, int certificado)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_POLIZA, poliza },
                { ParametersConstants.ID_CERTIFICADO, certificado }
            };
            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Moneda>(
                ProceduresConstants.SP_GETMONEDA,
                parametros,
                ParametersConstants.CURSOR_MON
            );
        }

        public async Task<ReglaAprobacion?> GetReglaAsync(double monto, int moneda)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.MONTO, monto },
                { ParametersConstants.MONEDA, moneda }
            };
            return await _dbConnection.QueryOracleFirstOrDefaultAsync<ReglaAprobacion>(
                ProceduresConstants.SP_REGLA,
                parametros,
                ParametersConstants.CURSOR_CO
            );
        }

        public async Task<Life?> GetPrimasAsync(long poliza, long certificado)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_POLIZA, poliza },
                { ParametersConstants.ID_CERTIFICADO, certificado }
            };
            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Life>(
                ProceduresConstants.SP_PRIMAS,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<RolesUsuario?> GetRolesAsync(long poliza, long certificado)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_POLIZA, poliza },
                { ParametersConstants.ID_CERTIFICADO, certificado }
            };
            return await _dbConnection.QueryOracleFirstOrDefaultAsync<RolesUsuario>(
                ProceduresConstants.SP_GETROLES,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<ParametrosCargaDocumentoExterno?> GetIdUsereAsync(string idusere)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_USERE, idusere }
            };
            return await _dbConnection.QueryOracleFirstOrDefaultAsync<ParametrosCargaDocumentoExterno>(
                ProceduresConstants.SP_GETIDUSUARIOEXTERNO,
                parametros,
                ParametersConstants.CURSOR_CO
            );
        }

        public async Task<IEnumerable<Correos>> GetCorreosRevisorAsync(double monto, int moneda)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.MONTO, monto },
                { ParametersConstants.MONEDA, moneda }
            };
            return await _dbConnection.QueryOracleAsync<Correos>(
                ProceduresConstants.SP_CORREOSREVISOR,
                parametros,
                ParametersConstants.CURSOR_CO
            );
        }

        public async Task<int> GetNuevoLoteAsync()
        {
            var parametros = new Dictionary<string, object>();
            var loteGenerador = await _dbConnection.QueryOracleFirstOrDefaultAsync<ParametrosGenerador>(
                ProceduresConstants.SP_NUEVO_LOTE,
                parametros,
                ParametersConstants.CURSOR_RET
            );

            if (loteGenerador == null || string.IsNullOrWhiteSpace(loteGenerador.lote))
            {
                return 1;
            }

            return int.TryParse(loteGenerador.lote, out int loteActual)
                ? loteActual + 1
                : 1;
        }

        public async Task<IEnumerable<Policy>> GetProductosAsync(int idDoc)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_DOCUMENTO, idDoc }
            };
            return await _dbConnection.QueryOracleAsync<Policy>(
                ProceduresConstants.SP_GETPRODUCTOSRAMODOC,
                parametros,
                ParametersConstants.CURSOR_RET
            );
        }

        public async Task<Policy?> GetRamoDocAsync(int idDoc)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_DOCUMENTO, idDoc }
            };
            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Policy>(
                ProceduresConstants.SP_GETRAMODOC,
                parametros,
                ParametersConstants.CURSOR_RET
            );
        }

        public async Task<Bitacora?> SetSinAprobacionBitacoraAsync(long idPoliza, string ruta, int estatus, string nombreArchivo, int idGenerico, int idUsuarioCreador, int idUsuario)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.ID_POLIZA, idPoliza },
                { ParametersConstants.RUTA, ruta },
                { ParametersConstants.NOMBRE_ARCHIVO, nombreArchivo },
                { ParametersConstants.ESTATUS, estatus },
                { ParametersConstants.ID_GENERICO, idGenerico },
                { ParametersConstants.ID_USUARIO_CREADOR, idUsuarioCreador },
                { ParametersConstants.ID_USUARIO, idUsuario }
            };

            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Bitacora>(
                ProceduresConstants.SP_SINAPROBITACORAALTA,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<IEnumerable<Correos>> GetCorreosRevisorGenNormalAsync(double monto, string poliza, long certificado)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.MONTO, monto },
                { ParametersConstants.ID_POLIZA, Convert.ToInt64(poliza) },
                { ParametersConstants.ID_CERTIFICADO, certificado }
            };
            return await _dbConnection.QueryOracleAsync<Correos>(
                ProceduresConstants.SP_CORREOSREVISORGENNORMAL,
                parametros,
                ParametersConstants.CURSOR_CO
            );
        }

        public async Task<CertificadoCampoFe?> SolicitudCertificadoAsync(long idPoliza, long certificado)
        {
            var parametros = new Dictionary<string, object>
    {
        { ParametersConstants.P_ID_POLIZA, idPoliza },
        { ParametersConstants.P_CERTIFICADO, certificado }
    };

            // 1. Dapper trae las filas como objetos dinámicos (la única forma segura con SYS_REFCURSOR en este caso)
            var rows = await _dbConnection.QueryOracleAsync<dynamic>(
                ProceduresConstants.SP_GETSOLICITUDCERTIFICADOTAGS,
                parametros,
                ParametersConstants.P_REF_CURSOR
            );

            if (rows == null || !rows.Any()) return null;

            var certificadoFe = new CertificadoCampoFe
            {
                NumeroPoliza = idPoliza,
                Certificado = certificado
            };

            // 2. Iteramos las filas de la tabla transpuesta
            foreach (var row in rows)
            {
                var item = (IDictionary<string, object>)row;
                var valores = item.Values.ToList();

                // Extraemos las columnas por posición (columna 0 = CAMPO, columna 1 = VALOR)
                string key = valores.ElementAtOrDefault(0)?.ToString()?.Trim() ?? string.Empty;
                string val = valores.ElementAtOrDefault(1)?.ToString()?.Trim() ?? string.Empty;

                // 3. El switch hace match perfecto con los tags definidos en RtfTemplateConstants y Helper
                switch (key)
                {
                    case RtfTemplateConstants.TAG_FECHA_EMISION:
                        if (DateTime.TryParseExact(val, SettingsConstants.DEFAULT_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fEmi))
                            certificadoFe.FechaEmision = fEmi;
                        break;

                    case RtfTemplateConstants.TAG_FECHA_VIGENCIA:
                        if (DateTime.TryParseExact(val, SettingsConstants.DEFAULT_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fVig))
                            certificadoFe.FechaVigencia = fVig;
                        break;

                    case RtfTemplateConstants.TAG_FECHA_FIN_VIGENCIA:
                        if (DateTime.TryParseExact(val, SettingsConstants.DEFAULT_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fFin))
                            certificadoFe.FechaFinVigencia = fFin;
                        break;

                    case RtfTemplateConstants.TAG_NOMBRE_ASEG:
                        certificadoFe.NombreAsegurado = val;
                        break;

                    case RtfTemplateConstants.TAG_DOC_ASEG:
                        certificadoFe.DocumentoAsegurado = val;
                        break;

                    case RtfTemplateConstants.TAG_EMAIL_ASEG:
                        certificadoFe.EmailAsegurado = val;
                        break;

                    case RtfTemplateConstants.TAG_FEC_NAC_ASEG:
                        if (DateTime.TryParseExact(val, SettingsConstants.DEFAULT_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fNac))
                            certificadoFe.FechaNacimientoAsegurado = fNac;
                        break;

                    case RtfTemplateConstants.TAG_EDAD_ASEG:
                        if (int.TryParse(val, out int edad))
                            certificadoFe.EdadAsegurado = edad;
                        break;

                    case RtfTemplateConstants.TAG_DIRECCION_ASEG:
                        certificadoFe.DireccionAsegurado = val;
                        break;

                    case RtfTemplateConstants.TAG_SEXO_ASEG:
                        certificadoFe.SexoAsegurado = val;
                        break;

                    case RtfTemplateConstants.TAG_NOMBRE_BEN:
                        certificadoFe.NombreBeneficiario = val;
                        break;

                    case RtfTemplateConstants.TAG_DOC_BEN:
                        certificadoFe.DocumentoBeneficiario = val;
                        break;

                    case RtfTemplateConstants.TAG_RELACION_BEN_ASEG:
                        certificadoFe.RelacionBeneficiario = val;
                        break;

                    case RtfTemplateConstants.TAG_TELEFONO_ASEG:
                        certificadoFe.TelefonoAsegurado = val;
                        break;

                    case RtfTemplateConstants.TAG_ES_DNI_ASEG:
                        certificadoFe.TipoDocAsegurado = val == "1" ? ParametersConstants.TIPO_DNI : ParametersConstants.TIPO_RUC;
                        break;

                    case RtfTemplateConstants.TAG_ES_DNI_BEN:
                        certificadoFe.TipoDocBeneficiario = val == "1" ? ParametersConstants.TIPO_DNI : ParametersConstants.TIPO_RUC;
                        break;

                    case RtfTemplateConstants.TAG_SCLIENT:
                        certificadoFe.SClientAsegurado = val;
                        break;

                    case RtfTemplateConstants.TAG_SCLIENT_BEN:
                        certificadoFe.SClientBeneficiario = val;
                        break;

                    case RtfTemplateConstants.TAG_PLAN_CERT:
                        if (int.TryParse(val, out int mod))
                            certificadoFe.NModulec = mod;
                        break;
                }
            }

            return certificadoFe;
        }

        public async Task RegistrarDocumentoRechazadoAsync(string nombreDocumento, string rutaDocumento, long idPoliza, long certificado, string plantilla)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.NOMBRE_DOCUMENTO, nombreDocumento },
                { ParametersConstants.RUTA_DOCUMENTO, rutaDocumento },
                { ParametersConstants.ID_POLIZA_CAPS, idPoliza },
                { ParametersConstants.CERTIFICADO_CAPS, certificado },
                { ParametersConstants.PLANTILLA, plantilla }
            };
            await _dbConnection.ExecuteOracleAsync(
                ProceduresConstants.SP_REGISTRARDOCUMENTORECHAZADO,
                parametros
            );
        }

        public async Task<DocumentoRTF?> ObtenerDocumentoRTFAsync(string nombreDoc)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.NOMBRE_DOC, nombreDoc }
            };
            return await _dbConnection.QueryOracleFirstOrDefaultAsync<DocumentoRTF>(
                ProceduresConstants.SP_GETDOCPORNOMBRE,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<CertificadoCajaLosAndes?> GetCertificadoTablaIntermediaTagsAsync(long idPoliza, long certificado, string sClient)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.P_ID_POLIZA, idPoliza },
                { ParametersConstants.P_CERTIFICADO, certificado },
                { ParametersConstants.P_VSCLIENT, sClient }
            };

            var rows = await _dbConnection.QueryOracleAsync<dynamic>(
                ProceduresConstants.SP_GET_CERTIFTABLAINTERMEDIATAGS,
                parametros,
                ParametersConstants.P_REF_CURSOR
            );

            if (rows == null || !rows.Any()) return null;

            var certLosAndes = new CertificadoCajaLosAndes
            {
                NumeroPoliza = idPoliza,
                Certificado = certificado
            };

            foreach (var row in rows)
            {
                var item = (IDictionary<string, object>)row;
                var valores = item.Values.ToList();

                string key = valores.ElementAtOrDefault(0)?.ToString()?.Trim() ?? string.Empty;
                string val = valores.ElementAtOrDefault(1)?.ToString()?.Trim() ?? string.Empty;

                switch (key)
                {
                    case RtfTemplateConstants.TAG_NOMBRE_ASEG: certLosAndes.NombreAsegurado = val; break;
                    case RtfTemplateConstants.TAG_DIRECCION_ASEG: certLosAndes.DireccionAsegurado = val; break;
                    case RtfTemplateConstants.TAG_SCLIENT: certLosAndes.SClientAsegurado = val; break;
                    case RtfTemplateConstants.TAG_DOC_ASEG: certLosAndes.DocumentoAsegurado = val; break;
                    case RtfTemplateConstants.TAG_NOMBRE_BEN1: certLosAndes.NombreBeneficiario1 = val; break;
                    case RtfTemplateConstants.TAG_DIRECCION_BEN1: certLosAndes.DireccionBeneficiario1 = val; break;
                    case RtfTemplateConstants.TAG_RELACION_BEN1: certLosAndes.RelacionBeneficiario1 = val; break;
                    case RtfTemplateConstants.TAG_PORC_BEN1: certLosAndes.PorcentajeBeneficiario1 = val; break;
                    case RtfTemplateConstants.TAG_NOMBRE_BEN2: certLosAndes.NombreBeneficiario2 = val; break;
                    case RtfTemplateConstants.TAG_DIRECCION_BEN2: certLosAndes.DireccionBeneficiario2 = val; break;
                    case RtfTemplateConstants.TAG_RELACION_BEN2: certLosAndes.RelacionBeneficiario2 = val; break;
                    case RtfTemplateConstants.TAG_PORC_BEN2: certLosAndes.PorcentajeBeneficiario2 = val; break;
                    case RtfTemplateConstants.TAG_PLAN_CERTIFICADO: certLosAndes.PlanCertificado = val; break;
                }
            }

            return certLosAndes;
        }

        public async Task<Client?> GetClavePdfAsync(string sClient)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.SCLIENT, sClient }
            };
            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Client>(
                ProceduresConstants.SP_GET_CLAVEPDF,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<Policy?> GetPolizaLosAndesAsync(long psPoliza, int psRamo)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.PS_RAMO, psRamo },
                { ParametersConstants.PS_POLIZA, psPoliza }
            };
            var policy = await _dbConnection.QueryOracleFirstOrDefaultAsync<Policy>(
                ProceduresConstants.SP_GET_CONFIG_REG_CLIENTES,
                parametros,
                ParametersConstants.CURSOR_RET
            );
            if (policy?.Tablaconfigurada != null)
            {
                policy.Tablaconfigurada = policy.Tablaconfigurada.Trim();
            }

            return policy;
        }

        public async Task<PolizaLosAndes?> GetCertificadoTablaIntermediaAsync(string psTablaconfigurada, long psPoliza, long psCertificado)
        {
            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.PS_POLIZA, psPoliza },
                { ParametersConstants.PS_CERTIFICADO, psCertificado }
            };

            var polizaLosAndes = await _dbConnection.QueryOracleFirstOrDefaultAsync<PolizaLosAndes>(
                ProceduresConstants.SP_GET_CERTIFICADO_LOS_ANDES,
                parametros,
                ParametersConstants.CURSOR_RET
            );

            if (polizaLosAndes != null)
            {
                polizaLosAndes.Npolicy = psPoliza;
                polizaLosAndes.Ncertif = psCertificado;
                polizaLosAndes.NRole = string.IsNullOrWhiteSpace(polizaLosAndes.NRole)
                    ? ParametersConstants.NROLE_DEFECTO
                    : polizaLosAndes.NRole.Trim();

                if (polizaLosAndes.Siddoc != null) polizaLosAndes.Siddoc = polizaLosAndes.Siddoc.Trim();
                if (polizaLosAndes.FechaEmision != null) polizaLosAndes.FechaEmision = polizaLosAndes.FechaEmision.Trim();
            }

            return polizaLosAndes;
        }

        public async Task<Client?> GetDataClientCertTablaIntermediaAsync(string psSiddoc)
        {

            var parametros = new Dictionary<string, object>
            {
                { ParametersConstants.PS_SID_DOC, psSiddoc }
            };
            return await _dbConnection.QueryOracleFirstOrDefaultAsync<Client>(
                ProceduresConstants.SP_GETCLIENTE_LOS_ANDES,
                parametros,
                ParametersConstants.CURSOR_OP
            );
        }

        public async Task<IEnumerable<Parametros>> GetApiPathsDataAsync()
        {
            var parametros = new Dictionary<string, object>();

            return await _dbConnection.QueryOracleAsync<Parametros>(
                ProceduresConstants.GET_API_PATHS_DATA,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task<IEnumerable<Parametros>> GetMailDataAsync()
        {
            var parametros = new Dictionary<string, object>();

            return await _dbConnection.QueryOracleAsync<Parametros>(
                ProceduresConstants.GET_MAIL_DATA,
                parametros,
                ParametersConstants.CURSOR
            );
        }

        public async Task InsertUsuariosAsync(string lote, Usuario usuario, string documentos)
        {
            var parametros = new Dictionary<string, object>
        {
            { ParametersConstants.P_LOTE, lote },
            { ParametersConstants.P_NOMBRE, usuario.Nombre },
            { ParametersConstants.P_APELLIDO, usuario.Apellido },
            { ParametersConstants.P_CORREO, usuario.Correo },
            { ParametersConstants.P_DNI_USER, usuario.DNI },
            { ParametersConstants.P_DOCUMENTOS, documentos }
        };

            await _dbConnection.ExecuteOracleAsync(
                ProceduresConstants.SP_INSERTUSEXTERNO,
                parametros
            );
        }
    }
}