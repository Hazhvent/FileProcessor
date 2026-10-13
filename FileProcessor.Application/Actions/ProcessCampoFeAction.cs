using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Repositories;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Utils;
using MediatR;

namespace FileProcessor.Application.Actions
{
    public sealed record ProcessCampoFeActionRequest(
        Documento Doc,
        ParametrosCargaDocumentoExterno DatosPlantilla,
        string LoteGenerado,
        List<Usuario> Usuarios
    ) : IRequest<ProcessCampoFeActionResponse>;

    public sealed record ProcessCampoFeActionResponse(
        byte[] PdfFinalBytes,
        Polizas PolizaUniversal,
        ParametrosCargaDocumentoExterno UsuarioAdminUniversal,
        List<Correos> CorreosRevisoresUniversal,
        string NombreArchivoUniversal,
        string ClavePdf
    );

    public class ProcessCampoFeActionHandler(
        IRepository repository,
        INetworkConnectionService networkConnectionService,
        IStorageService storageService,
        IPdfProcessorService pdfProcessorService,
        ISender sender)
        : IRequestHandler<ProcessCampoFeActionRequest, ProcessCampoFeActionResponse>
    {
        public async Task<ProcessCampoFeActionResponse> Handle(ProcessCampoFeActionRequest request, CancellationToken cancellationToken)
        {
            var doc = request.Doc;
            var datosPlantilla = request.DatosPlantilla;

            long polizaId = string.IsNullOrWhiteSpace(doc.Poliza) ? 0 : Convert.ToInt64(doc.Poliza);
            int certificadoId = string.IsNullOrWhiteSpace(doc.Certificado) ? 0 : Convert.ToInt32(doc.Certificado);
            string nombreRtf = string.Format(SettingsConstants.RTF_FILE_NAME_TEMPLATE, doc.NombrePlantilla.Trim());

            var polizaBD = await repository.GetPolizaAsync(polizaId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_POLICY_NOT_FOUND, polizaId));
            var certificadoPol = await repository.GetCertificadoAsync(polizaId, certificadoId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CERTIFICATE_NOT_FOUND, certificadoId, polizaId));
            var certificadoCampos = await repository.SolicitudCertificadoAsync(polizaId, certificadoId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CERTIFICATE_RTF_DATA_NOT_FOUND, certificadoId));
            var moneda = await repository.GetMonedaAsync(polizaId, certificadoId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CURRENCY_NOT_FOUND, polizaId));
            var primasVigencia = await repository.GetPrimasAsync(polizaId, certificadoId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_PREMIUMS_NOT_FOUND, polizaId));
            var contratante = await repository.GetDataClientAsync(polizaBD.SCLIENT) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CONTRACTOR_NOT_FOUND, polizaId));
            var usuarioAdminUniversal = await repository.GetIdUsereAsync(SettingsConstants.DEFAULT_SYSTEM_USER) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_ADMIN_USER_NOT_FOUND, SettingsConstants.DEFAULT_SYSTEM_USER));

            var approvalResult = await sender.Send(new GetApprovalRuleAndReviewersActionRequest(
                Capital: polizaBD.NCAPITAL,
                MonedaId: moneda.NCODIGINT,
                PolizaNumero: polizaBD.NPOLICY.ToString(),
                CertificadoId: certificadoId,
                EsConsultaNormal: true
            ), cancellationToken);

            var reglaAprobacion = approvalResult.ReglaAprobacion;
            var correosRevisoresUniversal = approvalResult.CorreosRevisores;

            DateTime fecStart = DateTime.TryParse(primasVigencia.DSTARTDATE, out var fs) ? fs : DateTime.Now;
            DateTime fecExp = DateTime.TryParse(primasVigencia.DEXPIRDAT, out var fe) ? fe : DateTime.Now;
            DateTime fecIss = DateTime.TryParse(certificadoPol.DISSUEDAT, out var fi) ? fi : DateTime.Now;

            string nombreAsegurado = !string.IsNullOrWhiteSpace(request.Usuarios.FirstOrDefault()?.Nombre)
                ? request.Usuarios.FirstOrDefault()!.Nombre.Trim()
                : certificadoCampos.NombreAsegurado;

            var policyResult = await sender.Send(new RegisterPolicyActionRequest(
                PolizaId: polizaId,
                CertificadoId: (int)certificadoPol.NCERTIF,
                RamoId: polizaBD.IDRAMO,
                ProductoId: polizaBD.NPRODUCT,
                Capital: polizaBD.NCAPITAL,
                Prima: certificadoPol.Prima,
                TipoDocId: datosPlantilla.IDTIPODOC,
                IdDocumento: datosPlantilla.IdDocumento,
                ReglaAprobacionId: reglaAprobacion.IDREGLAAPROBACION,
                MonedaId: moneda.NCODIGINT,
                Firma: datosPlantilla.Firma,
                LoteGenerado: request.LoteGenerado,
                GeneraSaludo: doc.GeneraSaludo,
                NombreContratante: contratante.SClieName,
                NombreAsegurado: nombreAsegurado,
                DocumentoAsegurado: certificadoCampos.DocumentoAsegurado,
                DireccionAsegurado: certificadoCampos.DireccionAsegurado,
                SexoAsegurado: certificadoCampos.SexoAsegurado,
                CorreoUsuario: request.Usuarios.FirstOrDefault()?.Correo ?? string.Empty,
                DocumentoContratante: contratante.SCient,
                MonedaSimbolo: moneda.SSHORT_DES,
                FechaInicioVigencia: fecStart,
                FechaFinVigencia: fecExp,
                FechaEmision: fecIss,
                FechaNacimientoAsegurado: certificadoCampos.FechaNacimientoAsegurado
            ), cancellationToken);

            var polizaUniversal = policyResult.PolizaRegistrada;

            var documentoRTF = await repository.ObtenerDocumentoRTFAsync(nombreRtf) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_TEMPLATE_NOT_FOUND, nombreRtf));
            var seccionesResult = await repository.GetSeccionesByIdAsync(datosPlantilla.IdDocumento);
            var secciones = seccionesResult?.ToList() ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_TEMPLATE_SECTIONS_NOT_FOUND, datosPlantilla.IdDocumento));

            string clavePDF = Helper.DetermineEncryptionKey(doc.DocContratante, string.Empty);
            if (string.IsNullOrWhiteSpace(clavePDF)) throw new Exceptions.BadRequestException(string.Format(ErrorConstants.REQUEST_ENCRYPTION_KEY_NOT_FOUND, polizaBD.SCLIENT));

            string fullRtfPath = Path.Combine(documentoRTF.RUTARTF, documentoRTF.NOMBREDOCUMENTO);
            string relativeRtf = Helper.ExtractInternalPath(fullRtfPath);

            string rtfContent = string.Empty;
            await networkConnectionService.RunInNetworkScope((fileStore) => { rtfContent = storageService.ReadNetworkTextFile(fileStore, relativeRtf); }, credentialMode: 1);

            if (string.IsNullOrWhiteSpace(rtfContent)) throw new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_RTF_FILE_EMPTY, documentoRTF.NOMBREDOCUMENTO));

            string rtfProcesado = Helper.ProcessRtfTemplateCampoFe(rtfContent, secciones, certificadoCampos);
            if (string.Equals(rtfContent, rtfProcesado, StringComparison.Ordinal)) throw new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_RTF_PROCESSING_FAILED, documentoRTF.NOMBREDOCUMENTO));

            using MemoryStream rtfStream = new(System.Text.Encoding.UTF8.GetBytes(rtfProcesado));
            using MemoryStream pdfRawStream = await pdfProcessorService.ConvertRtfToPdfStream(rtfStream);
            using MemoryStream pdfCifradoStream = pdfProcessorService.EncryptPdfStream(pdfRawStream, clavePDF);

            byte[] pdfFinalBytes = pdfCifradoStream.ToArray();
            string nombreBaseDocumento = string.Format(SettingsConstants.RTF_PDF_NAME_PATTERN, polizaUniversal.IDTBLGENERICA, polizaBD.IDRAMO, polizaBD.SCLIENT, certificadoId);
            string nombreArchivoUniversal = string.Format(SettingsConstants.PDF_FILE_NAME_TEMPLATE, nombreBaseDocumento);

            return new ProcessCampoFeActionResponse(
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