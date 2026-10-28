using FileProcessor.Application.Actions;
using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Repositories;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Models.Api;
using FileProcessor.Core.Models.Settings;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace FileProcessor.Application.UseCases
{
    public class ProcessDocumentBatchHandler(
            IOptions<PriorityWordsSettings> priorityWordsOptions,
            IRepository repository,
            ILoggingService<ProcessDocumentBatchHandler> logger,
            ISender sender)
            : IRequestHandler<ProcessDocumentBatchRequest, ProcessDocumentBatchResponse>
    {
        private readonly PriorityWordsSettings _priorityWordsSettings = priorityWordsOptions.Value;

        public async Task<ProcessDocumentBatchResponse> Handle(ProcessDocumentBatchRequest request, CancellationToken cancellationToken)
        {
            logger.LogInfo(LogsConstants.INFO_BATCH_STARTED, request.Documentos.Count, request.DirectoryPath ?? LogsConstants.LOG_VALUE_NOT_AVAILABLE);

            var docs = request.Documentos.OrderByDescending(doc =>
                _priorityWordsSettings.Keys.Any(kw =>
                    !string.IsNullOrWhiteSpace(kw) &&
                    doc.Nombre.Contains(kw, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            var correosAenviar = new List<string>();
            var plantillasValidadas = new Dictionary<string, ParametrosCargaDocumentoExterno>();
            var clavesPorDocumento = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var doc in docs)
            {
                string nombreRtf = string.Format(SettingsConstants.RTF_FILE_NAME_TEMPLATE, doc.NombrePlantilla.Trim());
                var datosPlantilla = await repository.GetDatosDocAsync(nombreRtf)
                    ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_TEMPLATE_NOT_FOUND, doc.NombrePlantilla));

                plantillasValidadas[doc.NombrePlantilla] = datosPlantilla;

                bool esImportado = datosPlantilla.TipoConsulta != 1 && datosPlantilla.TipoConsulta != 0;
                if (esImportado && string.IsNullOrWhiteSpace(request.DirectoryPath) && datosPlantilla.TipoAccion == 0)
                {
                    throw new Exceptions.BadRequestException(ErrorConstants.REQUEST_DIRECTORY_INCOMPLETE);
                }
            }

            logger.LogInfo(LogsConstants.INFO_TEMPLATES_VALIDATED, plantillasValidadas.Count);

            // 🌐 Preparación de infraestructura de red y rutas NAS
            var netDirResult = await sender.Send(new PrepareNetworkDirectoriesActionRequest(request.DirectoryPath), cancellationToken);

            logger.LogInfo(LogsConstants.INFO_DIRECTORIES_PREPARED);

            string rutaPendientesNasBase = netDirResult.RutaPendientesNasBase;
            string rutaPrioridadNasBase = netDirResult.RutaPrioridadNasBase;
            ServerPath? serverPathOrigen = netDirResult.ServerPathOrigen;
            var configuracionesRutasApi = netDirResult.ConfiguracionesRutasApi;

            string loteGenerado = (await repository.GetNuevoLoteAsync()).ToString();

            logger.LogInfo(LogsConstants.INFO_LOTE_GENERATED, loteGenerado);

            if (docs.Any(d => d.GeneraSaludo?.Trim().ToUpper() == ValidationConstants.GENERA_SALUDO_SI))
            {
                await sender.Send(new ProcessUserGreetingsActionRequest(request.Usuarios), cancellationToken);
            }

            foreach (var doc in docs)
            {
                long polizaId = string.IsNullOrWhiteSpace(doc.Poliza) ? 0 : Convert.ToInt64(doc.Poliza);
                int certificadoId = string.IsNullOrWhiteSpace(doc.Certificado) ? 0 : Convert.ToInt32(doc.Certificado);

                var datosPlantilla = plantillasValidadas[doc.NombrePlantilla];
                int tipoConsultaBD = datosPlantilla.TipoConsulta;
                bool esDocumentoImportado = tipoConsultaBD != 1 && tipoConsultaBD != 0;

                logger.LogInfo(LogsConstants.INFO_PROCESSING_DOCUMENT, doc.Nombre, polizaId, certificadoId);

                byte[]? pdfFinalBytes = null;
                bool requiereAprobacionUniversal = datosPlantilla.Aprobacion == 1;
                Polizas? polizaUniversal = null;
                ParametrosCargaDocumentoExterno? usuarioAdminUniversal = null;
                List<Correos> correosRevisoresUniversal = [];
                string rutaOrigenParaLimpiar = string.Empty;
                string nombreArchivoUniversal = string.Empty;

                // =========================================================================
                // EJECUCIÓN DE MACRO ACTIONS POR TIPO DE DOCUMENTO
                // =========================================================================
                if (esDocumentoImportado)
                {
                    logger.LogInfo(LogsConstants.FLOW_EXTERNAL_FILE);

                    var result1 = await sender.Send(new ProcessExternalFileActionRequest(
                        doc, datosPlantilla, serverPathOrigen, request.DirectoryPath!, loteGenerado, configuracionesRutasApi, request.Usuarios
                    ), cancellationToken);

                    if (result1.ArchivoRechazado)
                    {
                        logger.LogWarning(LogsConstants.WARN_FILE_REJECTED_SIZE, result1.NombreArchivoUniversal);
                        continue;
                    }

                    pdfFinalBytes = result1.PdfFinalBytes;
                    polizaUniversal = result1.PolizaUniversal;
                    usuarioAdminUniversal = result1.UsuarioAdminUniversal;
                    correosRevisoresUniversal = result1.CorreosRevisoresUniversal;
                    rutaOrigenParaLimpiar = result1.RutaOrigenParaLimpiar;
                    nombreArchivoUniversal = result1.NombreArchivoUniversal;
                    clavesPorDocumento[doc.Nombre.Trim()] = result1.ClavePdf;

                    logger.LogInfo(LogsConstants.INFO_FILE_READ_NAS, pdfFinalBytes!.Length);
                }
                else if (tipoConsultaBD == 1)
                {
                    logger.LogInfo(LogsConstants.FLOW_CAMPO_FE);

                    var result2 = await sender.Send(new ProcessCampoFeActionRequest(
                        doc, datosPlantilla, loteGenerado, request.Usuarios
                    ), cancellationToken);

                    pdfFinalBytes = result2.PdfFinalBytes;
                    polizaUniversal = result2.PolizaUniversal;
                    usuarioAdminUniversal = result2.UsuarioAdminUniversal;
                    correosRevisoresUniversal = result2.CorreosRevisoresUniversal;
                    nombreArchivoUniversal = result2.NombreArchivoUniversal;
                    clavesPorDocumento[doc.Nombre.Trim()] = result2.ClavePdf;

                    logger.LogInfo(LogsConstants.INFO_RTF_CONVERTED, nombreArchivoUniversal);
                }
                else if (tipoConsultaBD == 0)
                {
                    logger.LogInfo(LogsConstants.FLOW_CAJA_LOS_ANDES);

                    var result3 = await sender.Send(new ProcessCajaLosAndesActionRequest(
                        doc, datosPlantilla, loteGenerado, request.Usuarios
                    ), cancellationToken);

                    pdfFinalBytes = result3.PdfFinalBytes;
                    polizaUniversal = result3.PolizaUniversal;
                    usuarioAdminUniversal = result3.UsuarioAdminUniversal;
                    correosRevisoresUniversal = result3.CorreosRevisoresUniversal;
                    nombreArchivoUniversal = result3.NombreArchivoUniversal;
                    clavesPorDocumento[doc.Nombre.Trim()] = result3.ClavePdf;

                    logger.LogInfo(LogsConstants.INFO_RTF_CONVERTED, nombreArchivoUniversal);
                }

                // =========================================================================
                // ALMACENAMIENTO Y AUDITORÍA DEL DOCUMENTO
                // =========================================================================
                if (pdfFinalBytes != null && polizaUniversal != null && usuarioAdminUniversal != null)
                {
                    logger.LogInfo(LogsConstants.INFO_PDF_ENCRYPTED, nombreArchivoUniversal);

                    var storeResult = await sender.Send(new StoreAndAuditDocumentActionRequest(
                        PdfBytes: pdfFinalBytes,
                        PolizaUniversal: polizaUniversal,
                        UsuarioAdminUniversal: usuarioAdminUniversal,
                        CorreosRevisoresUniversal: correosRevisoresUniversal,
                        RequiereAprobacion: requiereAprobacionUniversal,
                        NombreArchivoUniversal: nombreArchivoUniversal,
                        RutaPendientesNasBase: rutaPendientesNasBase,
                        RutaPrioridadNasBase: rutaPrioridadNasBase,
                        RutaOrigenParaLimpiar: rutaOrigenParaLimpiar,
                        ServerPathOrigen: serverPathOrigen,
                        PolizaId: polizaId
                    ), cancellationToken);

                    string rutaNasElegida = requiereAprobacionUniversal ? rutaPendientesNasBase : rutaPrioridadNasBase;
                    logger.LogInfo(LogsConstants.INFO_DOCUMENT_SAVED_NAS, nombreArchivoUniversal, rutaNasElegida);
                    logger.LogInfo(LogsConstants.INFO_DATABASE_REGISTERED, nombreArchivoUniversal, requiereAprobacionUniversal ? LogsConstants.LOG_VALUE_YES : LogsConstants.LOG_VALUE_NO);

                    correosAenviar.AddRange(storeResult.CorreosAEnviar);
                }
            }

            await sender.Send(new SendPendingApprovalEmailsActionRequest(correosAenviar, loteGenerado), cancellationToken);

            if (correosAenviar.Count > 0)
            {
                logger.LogInfo(LogsConstants.INFO_EMAILS_SENT, correosAenviar.Distinct().Count());
            }

            await sender.Send(new RegisterBatchUsersActionRequest(request.Usuarios, loteGenerado, clavesPorDocumento), cancellationToken);

            logger.LogInfo(LogsConstants.INFO_USERS_REGISTERED, loteGenerado);

            logger.LogInfo(LogsConstants.INFO_BATCH_FINISHED, loteGenerado);

            return new ProcessDocumentBatchResponse(SettingsConstants.API_SUCCESS_RESPONSE_MESSAGE);
        }
    }

    public class ProcessDocumentBatchValidator : AbstractValidator<ProcessDocumentBatchRequest>
    {
        public ProcessDocumentBatchValidator()
        {
            // =========================================================================
            // 1. VALIDACIONES A NIVEL DE LOTE (REQUEST) - PARIDAD EXACTA CON LEGADO
            // =========================================================================
            RuleFor(x => x.Documentos)
                .NotEmpty().WithMessage(ErrorConstants.REQUEST_DOCUMENTS_INCOMPLETE);

            RuleFor(x => x.Usuarios)
                .NotEmpty().WithMessage(ErrorConstants.REQUEST_USERS_INCOMPLETE);

            RuleFor(x => x.DirectoryPath)
                .Must(path => string.IsNullOrWhiteSpace(path) || path.StartsWith(SettingsConstants.RTF_ESCAPED_BACKSLASH))
                .WithMessage(x => string.Format(ErrorConstants.REQUEST_UNC_PATH_INVALID, x.DirectoryPath));

            // =========================================================================
            // 2. VALIDACIONES A NIVEL DE CADA DOCUMENTO (validaDatosNull LEGADO)
            // =========================================================================
            RuleForEach(x => x.Documentos).ChildRules(doc =>
            {
                doc.RuleFor(d => d.Nombre)
                    .NotEmpty().WithMessage(ErrorConstants.REQUEST_DOCUMENT_NAME_REQUIRED);

                doc.RuleFor(d => d.NombrePlantilla)
                    .NotEmpty().WithMessage(ErrorConstants.REQUEST_TEMPLATE_NAME_REQUIRED);

                doc.RuleFor(d => d.Poliza)
                    .NotEmpty().WithMessage(ErrorConstants.REQUEST_POLICY_REQUIRED)
                    .Must(p => long.TryParse(p, out var val) && val > 0)
                    .WithMessage(ErrorConstants.REQUEST_POLICY_REQUIRED);

                // Campos opcionales iniciales según legado (se asignan defaults si vienen vacíos)
                doc.RuleFor(d => d.GeneraSaludo)
                    .Must(g => string.IsNullOrWhiteSpace(g) ||
                               g.Trim().ToUpper() == ValidationConstants.GENERA_SALUDO_SI ||
                               g.Trim().ToUpper() == ValidationConstants.GENERA_SALUDO_NO)
                    .WithMessage(ErrorConstants.REQUEST_GREETING_INVALID);

                doc.RuleFor(d => d.Ramo)
                    .Must(r => string.IsNullOrWhiteSpace(r) || (int.TryParse(r, out var val) && val > 0))
                    .WithMessage(ErrorConstants.REQUEST_RAMO_INVALID);

                doc.RuleFor(d => d.Producto)
                    .Must(p => string.IsNullOrWhiteSpace(p) || int.TryParse(p, out _))
                    .WithMessage(ErrorConstants.REQUEST_PRODUCTO_INVALID);
            });

            // =========================================================================
            // 3. VALIDACIONES A NIVEL DE CADA USUARIO (validaDatosNull LEGADO)
            // =========================================================================
            RuleForEach(x => x.Usuarios).ChildRules(user =>
            {
                user.RuleFor(u => u.Nombre)
                    .NotEmpty().WithMessage(ErrorConstants.REQUEST_USER_NAME_REQUIRED);

                user.RuleFor(u => u.Correo)
                    .NotEmpty().WithMessage(ErrorConstants.REQUEST_USER_EMAIL_INVALID)
                    .EmailAddress().WithMessage(ErrorConstants.REQUEST_USER_EMAIL_INVALID);
            });

            // Excepción condicional exacta para DNI del legado
            RuleForEach(x => x.Usuarios)
                .ChildRules(user =>
                {
                    user.RuleFor(u => u.DNI)
                        .NotEmpty().WithMessage(ErrorConstants.REQUEST_USER_DNI_REQUIRED);
                })
                .When(request => request.Documentos != null &&
                                !request.Documentos.Any(d => d.NombrePlantilla == ValidationConstants.TEMPLATE_CAMPO_FE));
        }
    }

    public sealed record ProcessDocumentBatchRequest(
        List<Documento> Documentos,
        List<Usuario> Usuarios,
        string DirectoryPath
    ) : IRequest<ProcessDocumentBatchResponse>;

    public sealed record ProcessDocumentBatchResponse(string Mensaje);
}