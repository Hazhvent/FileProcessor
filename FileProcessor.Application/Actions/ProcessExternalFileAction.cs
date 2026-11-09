using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Repositories;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Models.Api;
using FileProcessor.Core.Utils;
using MediatR;
using SMBLibrary;

namespace FileProcessor.Application.Actions
{
    public sealed record ProcessExternalFileActionRequest(
        Documento Doc,
        ParametrosCargaDocumentoExterno DatosPlantilla,
        ServerPath? ServerPathOrigen,
        string DirectoryPath,
        string LoteGenerado,
        IEnumerable<Parametros> ConfiguracionesRutasApi,
        List<Usuario> Usuarios
    ) : IRequest<ProcessExternalFileActionResponse>;

    public sealed record ProcessExternalFileActionResponse(
        byte[]? PdfFinalBytes,
        Polizas? PolizaUniversal,
        ParametrosCargaDocumentoExterno? UsuarioAdminUniversal,
        List<Correos> CorreosRevisoresUniversal,
        string RutaOrigenParaLimpiar,
        string NombreArchivoUniversal,
        bool ArchivoRechazado,
        string ClavePdf
    );

    public class ProcessExternalFileActionHandler(
        IRepository repository,
        INetworkConnectionService networkConnectionService,
        IStorageService storageService,
        IPdfProcessorService pdfProcessorService,
        ISender sender)
        : IRequestHandler<ProcessExternalFileActionRequest, ProcessExternalFileActionResponse>
    {
        public async Task<ProcessExternalFileActionResponse> Handle(ProcessExternalFileActionRequest request, CancellationToken cancellationToken)
        {
            var doc = request.Doc;
            var datosPlantilla = request.DatosPlantilla;
            var serverPathOrigen = request.ServerPathOrigen;

            long polizaId = string.IsNullOrWhiteSpace(doc.Poliza) ? 0 : Convert.ToInt64(doc.Poliza);
            int certificadoId = string.IsNullOrWhiteSpace(doc.Certificado) ? 0 : Convert.ToInt32(doc.Certificado);
            int ramoReq = string.IsNullOrWhiteSpace(doc.Ramo) ? -1 : Convert.ToInt32(doc.Ramo);
            int productoReq = string.IsNullOrWhiteSpace(doc.Producto) ? -1 : Convert.ToInt32(doc.Producto);
            double primaReq = string.IsNullOrWhiteSpace(doc.Prima) ? -1 : Convert.ToDouble(doc.Prima);
            double sumaAseguradaReq = string.IsNullOrWhiteSpace(doc.SumaAsegurada) ? 0 : Convert.ToDouble(doc.SumaAsegurada);

            string nombreArchivoUniversal = string.Format(SettingsConstants.PDF_FILE_NAME_TEMPLATE, doc.Nombre.Trim());

            if (serverPathOrigen == null)
            {
                serverPathOrigen = Helper.ParseServerPath(request.DirectoryPath)
                    ?? throw new Exceptions.BadRequestException(ErrorConstants.REQUEST_DIRECTORY_INCOMPLETE);
            }

            long? fileSizeBytes = null;
            string rutaArchivoNas = Path.Combine(serverPathOrigen.RelativePath, nombreArchivoUniversal);

            await networkConnectionService.RunInNetworkScope(serverPathOrigen.Host, serverPathOrigen.ShareFolder, (fileStore) =>
            {
                fileSizeBytes = storageService.GetNetworkFileSize(fileStore, rutaArchivoNas);
            }, credentialMode: 0);

            if (fileSizeBytes == null)
                throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_IMPORTED_FILE_NOT_FOUND, nombreArchivoUniversal, rutaArchivoNas));

            if (Helper.ConvertBytesToMegabytes(fileSizeBytes.Value) > ValidationConstants.MAX_FILE_SIZE_MB)
            {
                string rutaNoEnviadosBase = Helper.GetRouteOrThrow(request.ConfiguracionesRutasApi, SettingsConstants.DB_PARAM_NAS_RUTA_NOE);
                string relativeNoeBase = Helper.ExtractInternalPath(rutaNoEnviadosBase);
                string carpetaFechaHoy = DateTime.Now.ToString(SettingsConstants.FOLDER_DATE_FORMAT);
                string rutaDirectorioRechazoNas = Path.Combine(relativeNoeBase, carpetaFechaHoy);

                byte[]? rawRejectedBytes = null;
                await networkConnectionService.RunInNetworkScope(serverPathOrigen.Host, serverPathOrigen.ShareFolder, (fileStore) =>
                {
                    long fileSize = fileSizeBytes.Value;
                    using MemoryStream ms = new();
                    NTStatus openReadStatus = fileStore.CreateFile(out object readHandle, out _, rutaArchivoNas, AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null);
                    if (openReadStatus == NTStatus.STATUS_SUCCESS)
                    {
                        try
                        {
                            long bytesLeidos = 0;
                            while (bytesLeidos < fileSize)
                            {
                                int bytesToRead = Helper.CalculateNextChunkSize(fileSize, bytesLeidos, SettingsConstants.SMB_READ_CHUNK_SIZE);
                                NTStatus readChunkStatus = fileStore.ReadFile(out byte[] buffer, readHandle, bytesLeidos, bytesToRead);
                                if (buffer != null && buffer.Length > 0)
                                {
                                    ms.Write(buffer, 0, buffer.Length);
                                    bytesLeidos += buffer.Length;
                                }
                            }
                        }
                        finally { fileStore.CloseFile(readHandle); }
                        rawRejectedBytes = ms.ToArray();
                    }
                }, credentialMode: 0);

                if (rawRejectedBytes != null)
                {
                    await networkConnectionService.RunInNetworkScope((fileStore) =>
                    {
                        storageService.EnsureNetworkDirectoriesExist(fileStore, [new Parametros { Parametro = rutaDirectorioRechazoNas }]);
                        string rutaArchivoDestinoNas = Path.Combine(rutaDirectorioRechazoNas, nombreArchivoUniversal);
                        storageService.WriteNetworkFile(fileStore, rutaArchivoDestinoNas, rawRejectedBytes);
                    }, credentialMode: 1);

                    await networkConnectionService.RunInNetworkScope(serverPathOrigen.Host, serverPathOrigen.ShareFolder, (fileStore) =>
                    {
                        storageService.DeleteNetworkFile(fileStore, rutaArchivoNas);
                    }, credentialMode: 0);
                }

                string rutaDestinoFinalRechazoDB = Path.Combine(rutaNoEnviadosBase, carpetaFechaHoy, nombreArchivoUniversal);
                await repository.RegistrarDocumentoRechazadoAsync(nombreArchivoUniversal, rutaDestinoFinalRechazoDB, polizaId, certificadoId, doc.NombrePlantilla);

                return new ProcessExternalFileActionResponse(
                    PdfFinalBytes: null,
                    PolizaUniversal: null,
                    UsuarioAdminUniversal: null,
                    CorreosRevisoresUniversal: [],
                    RutaOrigenParaLimpiar: string.Empty,
                    NombreArchivoUniversal: nombreArchivoUniversal,
                    ArchivoRechazado: true,
                    ClavePdf: string.Empty
                );
            }

            bool isExternalPolicy = !(ramoReq == -1 && productoReq == -1 && primaReq == -1);
            Polizas polizaUniversal;
            ParametrosCargaDocumentoExterno usuarioAdminUniversal;
            List<Correos> correosRevisoresUniversal;
            string clavePdf;

            if (!isExternalPolicy)
            {
                var polizaBD = await repository.GetPolizaAsync(polizaId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_POLICY_NOT_FOUND, polizaId));

                await ValidarReglasNegocioFlujoStandardAsync(polizaBD.IDRAMO, polizaBD.NPRODUCT, datosPlantilla);

                var certificadoBD = await repository.GetCertificadoAsync(polizaId, certificadoId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CERTIFICATE_NOT_FOUND, certificadoId, polizaId));
                var rolesAsegurado = await repository.GetRolesAsync(polizaId, certificadoId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_INSURED_ROLE_NOT_FOUND, polizaId));
                var moneda = await repository.GetMonedaAsync(polizaId, certificadoId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CURRENCY_NOT_FOUND, polizaId));
                var primasVigencia = await repository.GetPrimasAsync(polizaId, certificadoId) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_PREMIUMS_NOT_FOUND, polizaId));
                usuarioAdminUniversal = await repository.GetIdUsereAsync(SettingsConstants.DEFAULT_SYSTEM_USER) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_ADMIN_USER_NOT_FOUND, SettingsConstants.DEFAULT_SYSTEM_USER));

                string idContratante = (doc.GeneraSaludo.Trim().Equals(ValidationConstants.GENERA_SALUDO_SI, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrWhiteSpace(doc.DocContratante)) ? doc.DocContratante : polizaBD.SCLIENT;
                var contratante = await repository.GetDataClientAsync(idContratante) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CONTRACTOR_NOT_FOUND, polizaId));
                var asegurado = await repository.GetDataClientAsync(rolesAsegurado.SCLIENT) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_INSURED_NOT_FOUND, polizaId));
                var direccionesAseguradoResult = await repository.GetAddressAsync(rolesAsegurado.SCLIENT);
                var direccionesAsegurado = direccionesAseguradoResult?.ToList() ?? [];

                var approvalResult = await sender.Send(new GetApprovalRuleAndReviewersActionRequest(
                    Capital: polizaBD.NCAPITAL,
                    MonedaId: moneda.NCODIGINT
                ), cancellationToken);

                var reglaAprobacion = approvalResult.ReglaAprobacion;
                correosRevisoresUniversal = approvalResult.CorreosRevisores;

                DateTime fecStart = DateTime.TryParse(primasVigencia?.DSTARTDATE, out var fs) ? fs : DateTime.Now;
                DateTime fecExp = DateTime.TryParse(primasVigencia?.DEXPIRDAT, out var fe) ? fe : DateTime.Now;
                DateTime fecIss = DateTime.TryParse(certificadoBD.DISSUEDAT, out var fi) ? fi : DateTime.Now;
                DateTime? fecBirth = DateTime.TryParse(asegurado.DBIRTHDAT, out var fb) ? fb : null;

                string nombreAsegurado = !string.IsNullOrWhiteSpace(request.Usuarios.FirstOrDefault()?.Nombre)
                    ? request.Usuarios.FirstOrDefault()!.Nombre.Trim()
                    : asegurado.SClieName;

                var policyResult = await sender.Send(new RegisterPolicyActionRequest(
                    PolizaId: polizaId,
                    CertificadoId: (int)certificadoBD.NCERTIF,
                    RamoId: polizaBD.IDRAMO,
                    ProductoId: polizaBD.NPRODUCT,
                    Capital: polizaBD.NCAPITAL,
                    Prima: certificadoBD.Prima,
                    TipoDocId: datosPlantilla.IDTIPODOC,
                    IdDocumento: datosPlantilla.IdDocumento,
                    ReglaAprobacionId: reglaAprobacion.IDREGLAAPROBACION,
                    MonedaId: moneda.NCODIGINT,
                    Firma: datosPlantilla.Firma,
                    LoteGenerado: request.LoteGenerado,
                    GeneraSaludo: doc.GeneraSaludo,
                    NombreContratante: contratante.SClieName,
                    NombreAsegurado: nombreAsegurado,
                    DocumentoAsegurado: asegurado.SCUIT,
                    DireccionAsegurado: direccionesAsegurado.Count >= 1 ? direccionesAsegurado[0].SSTREET : ValidationConstants.LEGACY_EMPTY_PLACEHOLDER,
                    SexoAsegurado: asegurado.SSEXCLIEN,
                    CorreoUsuario: request.Usuarios.FirstOrDefault()?.Correo ?? string.Empty,
                    DocumentoContratante: contratante.SCient,
                    MonedaSimbolo: moneda.SSHORT_DES,
                    FechaInicioVigencia: fecStart,
                    FechaFinVigencia: fecExp,
                    FechaEmision: fecIss,
                    FechaNacimientoAsegurado: fecBirth
                ), cancellationToken);

                polizaUniversal = policyResult.PolizaRegistrada;

                var clavePdfResult = await repository.GetClavePdfAsync(polizaBD.SCLIENT);
                string rawClave = clavePdfResult?.clave ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_ENCRYPTION_KEY_NOT_FOUND, polizaBD.SCLIENT));
                clavePdf = Helper.DetermineEncryptionKey(rawClave, string.Empty);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(doc.DocContratante))
                    throw new Exceptions.BadRequestException(ErrorConstants.REQUEST_CONTRACTOR_DOC_REQUIRED);

                await ValidarReglasNegocioFlujoStandardAsync(ramoReq, productoReq, datosPlantilla);

                usuarioAdminUniversal = await repository.GetIdUsereAsync(SettingsConstants.DEFAULT_SYSTEM_USER) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_ADMIN_USER_NOT_FOUND, SettingsConstants.DEFAULT_SYSTEM_USER));

                var approvalResult = await sender.Send(new GetApprovalRuleAndReviewersActionRequest(
                    Capital: sumaAseguradaReq,
                    MonedaId: datosPlantilla.IdMoneda
                ), cancellationToken);

                var reglaAprobacion = approvalResult.ReglaAprobacion;
                correosRevisoresUniversal = approvalResult.CorreosRevisores;

                var contratante = await repository.GetDataClientAsync(doc.DocContratante) ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_CONTRACTOR_NOT_FOUND, doc.DocContratante));

                DateTime fechaActual = DateTime.Now;
                string nombreUsuario = request.Usuarios.FirstOrDefault()?.Nombre?.Trim() ?? string.Empty;
                string apellidoUsuario = request.Usuarios.FirstOrDefault()?.Apellido?.Trim() ?? string.Empty;
                string nombreCompleto = string.Format(ValidationConstants.FULL_NAME_FORMAT, nombreUsuario, apellidoUsuario).Trim();
                if (string.IsNullOrEmpty(nombreCompleto)) nombreCompleto = contratante.SClieName;

                var policyResult = await sender.Send(new RegisterPolicyActionRequest(
                    PolizaId: polizaId,
                    CertificadoId: certificadoId,
                    RamoId: ramoReq,
                    ProductoId: productoReq,
                    Capital: sumaAseguradaReq,
                    Prima: 0,
                    TipoDocId: datosPlantilla.IDTIPODOC,
                    IdDocumento: datosPlantilla.IdDocumento,
                    ReglaAprobacionId: reglaAprobacion.IDREGLAAPROBACION,
                    MonedaId: datosPlantilla.IdMoneda,
                    Firma: datosPlantilla.Firma,
                    LoteGenerado: request.LoteGenerado,
                    GeneraSaludo: doc.GeneraSaludo,
                    NombreContratante: contratante.SClieName,
                    NombreAsegurado: nombreCompleto,
                    DocumentoAsegurado: ValidationConstants.LEGACY_EMPTY_PLACEHOLDER,
                    DireccionAsegurado: ValidationConstants.LEGACY_EMPTY_PLACEHOLDER,
                    SexoAsegurado: ValidationConstants.LEGACY_EMPTY_PLACEHOLDER,
                    CorreoUsuario: request.Usuarios.FirstOrDefault()?.Correo ?? string.Empty,
                    DocumentoContratante: contratante.SCient,
                    MonedaSimbolo: ValidationConstants.LEGACY_EMPTY_PLACEHOLDER,
                    FechaInicioVigencia: fechaActual,
                    FechaFinVigencia: fechaActual,
                    FechaEmision: fechaActual,
                    FechaNacimientoAsegurado: null
                ), cancellationToken);

                polizaUniversal = policyResult.PolizaRegistrada;
                clavePdf = Helper.DetermineEncryptionKey(doc.DocContratante, string.Empty);
            }

            byte[]? pdfFinalBytes = null;
            string rutaOrigenParaLimpiar = rutaArchivoNas;

            await networkConnectionService.RunInNetworkScope(serverPathOrigen.Host, serverPathOrigen.ShareFolder, (fileStore) =>
            {
                long fileSize = storageService.GetNetworkFileSize(fileStore, rutaOrigenParaLimpiar) ?? throw new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMB_FILE_SIZE_NULL);
                using MemoryStream pdfOriginalBuffer = new();
                NTStatus openReadStatus = fileStore.CreateFile(out object readHandle, out _, rutaOrigenParaLimpiar, AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null);
                if (openReadStatus != NTStatus.STATUS_SUCCESS) throw new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, openReadStatus));

                try
                {
                    long bytesLeidos = 0;
                    while (bytesLeidos < fileSize)
                    {
                        int bytesToRead = Helper.CalculateNextChunkSize(fileSize, bytesLeidos, SettingsConstants.SMB_READ_CHUNK_SIZE);
                        NTStatus readChunkStatus = fileStore.ReadFile(out byte[] buffer, readHandle, bytesLeidos, bytesToRead);
                        if (buffer != null && buffer.Length > 0)
                        {
                            pdfOriginalBuffer.Write(buffer, 0, buffer.Length);
                            bytesLeidos += buffer.Length;
                        }
                    }
                }
                finally { fileStore.CloseFile(readHandle); }

                pdfOriginalBuffer.Position = 0;
                using MemoryStream pdfCifradoBuffer = pdfProcessorService.EncryptPdfStream(pdfOriginalBuffer, clavePdf);
                pdfFinalBytes = pdfCifradoBuffer.ToArray();
            }, credentialMode: 0);

            return new ProcessExternalFileActionResponse(
                PdfFinalBytes: pdfFinalBytes,
                PolizaUniversal: polizaUniversal,
                UsuarioAdminUniversal: usuarioAdminUniversal,
                CorreosRevisoresUniversal: correosRevisoresUniversal,
                RutaOrigenParaLimpiar: rutaOrigenParaLimpiar,
                NombreArchivoUniversal: nombreArchivoUniversal,
                ArchivoRechazado: false,
                ClavePdf: clavePdf
            );
        }

        private async Task ValidarReglasNegocioFlujoStandardAsync(int ramoId, int productoId, ParametrosCargaDocumentoExterno datosPlantilla)
        {
            var businessErrors = new List<ApiSubError>();

            if (datosPlantilla.ID_PRODUCTO != -2)
            {
                var productosPermitidos = await repository.GetProductosAsync(datosPlantilla.IdDocumento);
                if (productosPermitidos == null || !productosPermitidos.Any(p => p.NPRODUCT == productoId))
                    businessErrors.Add(new ApiSubError(ErrorConstants.PROPERTY_PRODUCTO, ErrorConstants.REQUEST_BUSINESS_PRODUCT_MISMATCH));
            }
            else
            {
                var ramoPlantilla = await repository.GetRamoDocAsync(datosPlantilla.IdDocumento);
                if (ramoPlantilla == null || ramoPlantilla.IDRAMO != ramoId)
                    businessErrors.Add(new ApiSubError(ErrorConstants.PROPERTY_RAMO, ErrorConstants.REQUEST_BUSINESS_BRANCH_MISMATCH));
            }

            if (businessErrors.Count != 0) throw new Exceptions.UnprocessableEntityException(businessErrors);
        }
    }
}