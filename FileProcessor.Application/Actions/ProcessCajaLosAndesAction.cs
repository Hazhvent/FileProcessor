using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Repositories;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Utils;
using MediatR;
using System.Globalization;

namespace FileProcessor.Application.Actions
{
    public sealed record ProcessCajaLosAndesActionRequest(
        Documento Doc,
        ParametrosCargaDocumentoExterno DatosPlantilla,
        string LoteGenerado,
        List<Usuario> Usuarios
    ) : IRequest<ProcessCajaLosAndesActionResponse>;

    public sealed record ProcessCajaLosAndesActionResponse(
        byte[] PdfFinalBytes,
        Polizas PolizaUniversal,
        ParametrosCargaDocumentoExterno UsuarioAdminUniversal,
        List<Correos> CorreosRevisoresUniversal,
        string NombreArchivoUniversal,
        string ClavePdf
    );

    public class ProcessCajaLosAndesActionHandler(
        IRepository repository,
        INetworkConnectionService networkConnectionService,
        IStorageService storageService,
        IPdfProcessorService pdfProcessorService,
        ISender sender)
        : IRequestHandler<ProcessCajaLosAndesActionRequest, ProcessCajaLosAndesActionResponse>
    {
        public async Task<ProcessCajaLosAndesActionResponse> Handle(ProcessCajaLosAndesActionRequest request, CancellationToken cancellationToken)
        {
            var doc = request.Doc;
            var datosPlantilla = request.DatosPlantilla;

            long polizaId = string.IsNullOrWhiteSpace(doc.Poliza) ? 0 : Convert.ToInt64(doc.Poliza);
            int certificadoId = string.IsNullOrWhiteSpace(doc.Certificado) ? 0 : Convert.ToInt32(doc.Certificado);
            int ramoReq = string.IsNullOrWhiteSpace(doc.Ramo) ? -1 : Convert.ToInt32(doc.Ramo);
            int productoReq = string.IsNullOrWhiteSpace(doc.Producto) ? -1 : Convert.ToInt32(doc.Producto);
            string nombreRtf = string.Format(SettingsConstants.RTF_FILE_NAME_TEMPLATE, doc.NombrePlantilla.Trim());

            var configPoliza = await repository.GetPolizaLosAndesAsync(polizaId, ramoReq) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_POLICY_NOT_FOUND, polizaId));
            if (string.IsNullOrWhiteSpace(configPoliza.Tablaconfigurada)) throw new Exceptions.BadRequestException(string.Format(ErrorConstants.REQUEST_POLICY_NOT_FOUND, polizaId));

            var polizaBD = await repository.GetPolizaAsync(polizaId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_POLICY_NOT_FOUND, polizaId));
            var polizaLosAndes = await repository.GetCertificadoTablaIntermediaAsync(configPoliza.Tablaconfigurada, polizaId, certificadoId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CERTIFICATE_NOT_FOUND, certificadoId, polizaId));
            var certLosAndes = await repository.GetCertificadoTablaIntermediaTagsAsync(polizaId, certificadoId, polizaLosAndes.Siddoc) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CERTIFICATE_RTF_DATA_NOT_FOUND, certificadoId));

            DateTime fechaEmisionParseada = DateTime.TryParseExact(polizaLosAndes.FechaEmision, SettingsConstants.INPUT_DATE_FORMAT_YYYYMMDD, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fParsed) ? fParsed : DateTime.Now;
            certLosAndes.FechaEmision = fechaEmisionParseada;

            var clientLosAndes = await repository.GetDataClientCertTablaIntermediaAsync(polizaLosAndes.Siddoc);
            var moneda = await repository.GetMonedaAsync(polizaId, certificadoId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CURRENCY_NOT_FOUND, polizaId));
            var usuarioAdminUniversal = await repository.GetIdUsereAsync(SettingsConstants.DEFAULT_SYSTEM_USER) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_ADMIN_USER_NOT_FOUND, SettingsConstants.DEFAULT_SYSTEM_USER));

            var approvalResult = await sender.Send(new GetApprovalRuleAndReviewersActionRequest(
                Capital: polizaLosAndes.NCAPITAL,
                MonedaId: moneda.NCODIGINT
            ), cancellationToken);

            var reglaAprobacion = approvalResult.ReglaAprobacion;
            var correosRevisoresUniversal = approvalResult.CorreosRevisores;

            DateTime fechaActual = DateTime.Now;
            string nombreAsegurado = !string.IsNullOrWhiteSpace(request.Usuarios.FirstOrDefault()?.Nombre)
                ? request.Usuarios.FirstOrDefault()!.Nombre.Trim()
                : certLosAndes.NombreAsegurado;

            var policyResult = await sender.Send(new RegisterPolicyActionRequest(
                PolizaId: polizaId,
                CertificadoId: (int)polizaLosAndes.Ncertif,
                RamoId: ramoReq,
                ProductoId: productoReq,
                Capital: polizaLosAndes.NCAPITAL,
                Prima: 0,
                TipoDocId: datosPlantilla.IDTIPODOC,
                IdDocumento: datosPlantilla.IdDocumento,
                ReglaAprobacionId: reglaAprobacion.IDREGLAAPROBACION,
                MonedaId: moneda.NCODIGINT,
                Firma: datosPlantilla.Firma,
                LoteGenerado: request.LoteGenerado,
                GeneraSaludo: doc.GeneraSaludo,
                NombreContratante: clientLosAndes?.SClieName ?? certLosAndes.NombreAsegurado,
                NombreAsegurado: nombreAsegurado,
                DocumentoAsegurado: certLosAndes.DocumentoAsegurado,
                DireccionAsegurado: certLosAndes.DireccionAsegurado,
                SexoAsegurado: ValidationConstants.LEGACY_EMPTY_PLACEHOLDER,
                CorreoUsuario: request.Usuarios.FirstOrDefault()?.Correo ?? string.Empty,
                DocumentoContratante: clientLosAndes?.SCient ?? ValidationConstants.LEGACY_EMPTY_PLACEHOLDER,
                MonedaSimbolo: ValidationConstants.LEGACY_EMPTY_PLACEHOLDER,
                FechaInicioVigencia: fechaEmisionParseada,
                FechaFinVigencia: fechaActual,
                FechaEmision: fechaActual,
                FechaNacimientoAsegurado: null
            ), cancellationToken);

            var polizaUniversal = policyResult.PolizaRegistrada;

            var documentoRTF = await repository.ObtenerDocumentoRTFAsync(nombreRtf) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_TEMPLATE_NOT_FOUND, nombreRtf));
            var seccionesResult = await repository.GetSeccionesByIdAsync(datosPlantilla.IdDocumento);
            var secciones = seccionesResult?.ToList() ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_TEMPLATE_SECTIONS_NOT_FOUND, datosPlantilla.IdDocumento));

            string clavePDF = Helper.DetermineEncryptionKey(doc.DocContratante, certLosAndes.DocumentoAsegurado);
            if (string.IsNullOrWhiteSpace(clavePDF)) throw new Exceptions.BadRequestException(string.Format(ErrorConstants.REQUEST_ENCRYPTION_KEY_NOT_FOUND, polizaBD.SCLIENT));

            string fullRtfPath = Path.Combine(documentoRTF.RUTARTF, documentoRTF.NOMBREDOCUMENTO);
            string relativeRtf = Helper.ExtractInternalPath(fullRtfPath);

            string rtfContent = string.Empty;
            await networkConnectionService.RunInNetworkScope((fileStore) => { rtfContent = storageService.ReadNetworkTextFile(fileStore, relativeRtf); }, credentialMode: 1);

            string rtfProcesado = Helper.ProcessRtfTemplateCajaLosAndes(rtfContent, secciones, certLosAndes);

            using MemoryStream rtfStream = new(System.Text.Encoding.UTF8.GetBytes(rtfProcesado));
            using MemoryStream pdfRawStream = await pdfProcessorService.ConvertRtfToPdfStream(rtfStream);
            using MemoryStream pdfCifradoStream = pdfProcessorService.EncryptPdfStream(pdfRawStream, clavePDF);

            byte[] pdfFinalBytes = pdfCifradoStream.ToArray();
            string nombreBaseDocumento = string.Format(SettingsConstants.RTF_PDF_NAME_PATTERN, polizaUniversal.IDTBLGENERICA, ramoReq, polizaLosAndes.Siddoc, certificadoId);
            string nombreArchivoUniversal = string.Format(SettingsConstants.PDF_FILE_NAME_TEMPLATE, nombreBaseDocumento);

            return new ProcessCajaLosAndesActionResponse(
                PdfFinalBytes: pdfFinalBytes,
                PolizaUniversal: polizaUniversal,
                UsuarioAdminUniversal: usuarioAdminUniversal,
                CorreosRevisoresUniversal: correosRevisoresUniversal,
                NombreArchivoUniversal: nombreArchivoUniversal,
                ClavePdf: clavePDF
            );
        }
    }
}