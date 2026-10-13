using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Database;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Interfaces.Repositories;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Models.Api;
using FileProcessor.Core.Utils;
using MediatR;
using SMBLibrary.Client;

namespace FileProcessor.Application.Actions
{
    public sealed record StoreAndAuditDocumentActionRequest(
        byte[] PdfBytes,
        Polizas PolizaUniversal,
        ParametrosCargaDocumentoExterno UsuarioAdminUniversal,
        List<Correos> CorreosRevisoresUniversal,
        bool RequiereAprobacion,
        string NombreArchivoUniversal,
        string RutaPendientesNasBase,
        string RutaPrioridadNasBase,
        string RutaOrigenParaLimpiar,
        ServerPath? ServerPathOrigen,
        long PolizaId
    ) : IRequest<StoreAndAuditDocumentActionResponse>;

    public sealed record StoreAndAuditDocumentActionResponse(
        List<string> CorreosAEnviar
    );

    public class StoreAndAuditDocumentActionHandler(
        IRepository repository,
        INetworkConnectionService networkConnectionService,
        IStorageService storageService)
        : IRequestHandler<StoreAndAuditDocumentActionRequest, StoreAndAuditDocumentActionResponse>
    {
        public async Task<StoreAndAuditDocumentActionResponse> Handle(StoreAndAuditDocumentActionRequest request, CancellationToken cancellationToken)
        {
            var correosNuevos = new List<string>();

            string rutaNasElegida = request.RequiereAprobacion ? request.RutaPendientesNasBase : request.RutaPrioridadNasBase;
            string relativeDestinoBase = Helper.ExtractInternalPath(rutaNasElegida);

            void savePdfAction(ISMBFileStore fileStore)
            {
                string rutaDestinoNas = Path.Combine(relativeDestinoBase, request.NombreArchivoUniversal);
                storageService.WriteNetworkFile(fileStore, rutaDestinoNas, request.PdfBytes);
            }

            await networkConnectionService.RunInNetworkScope(savePdfAction, credentialMode: 1);


            if (!string.IsNullOrWhiteSpace(request.RutaOrigenParaLimpiar) && request.ServerPathOrigen != null)
            {
                await networkConnectionService.RunInNetworkScope(request.ServerPathOrigen.Host, request.ServerPathOrigen.ShareFolder, (fileStore) =>
                {
                    storageService.DeleteNetworkFile(fileStore, request.RutaOrigenParaLimpiar);
                }, credentialMode: 0);
            }

            if (!request.RequiereAprobacion)
            {
                await repository.SetSinAprobacionAsync(
                    request.PolizaId,
                    request.UsuarioAdminUniversal.ID_USUARIO,
                    StatusesConstants.PRIORIDAD,
                    request.NombreArchivoUniversal,
                    request.PolizaUniversal.IDTBLGENERICA,
                    request.UsuarioAdminUniversal.ID_USUARIO,
                    SettingsConstants.DEFAULT_POLIZA_EMITIDA
                );

                await repository.SetSinAprobacionBitacoraAsync(
                    request.PolizaId,
                    rutaNasElegida,
                    StatusesConstants.PRIORIDAD,
                    request.NombreArchivoUniversal,
                    request.PolizaUniversal.IDTBLGENERICA,
                    request.UsuarioAdminUniversal.ID_USUARIO,
                    request.UsuarioAdminUniversal.ID_USUARIO
                );
            }
            else
            {
                foreach (var revisor in request.CorreosRevisoresUniversal)
                {
                    await repository.SetAprobacionAsync(
                        request.PolizaId,
                        revisor.IDUSUARIO,
                        StatusesConstants.PENDIENTE,
                        request.NombreArchivoUniversal,
                        request.PolizaUniversal.IDTBLGENERICA,
                        request.UsuarioAdminUniversal.ID_USUARIO,
                        SettingsConstants.DEFAULT_POLIZA_ASIGNADA,
                        SettingsConstants.DEFAULT_POLIZA_EMITIDA
                    );

                    await repository.SetBitacoraAsync(
                        request.PolizaId,
                        rutaNasElegida,
                        StatusesConstants.PENDIENTE,
                        request.NombreArchivoUniversal,
                        request.PolizaUniversal.IDTBLGENERICA,
                        request.UsuarioAdminUniversal.ID_USUARIO,
                        revisor.IDUSUARIO
                    );

                    if (!string.IsNullOrWhiteSpace(revisor.SE_MAIL))
                    {
                        correosNuevos.Add(revisor.SE_MAIL.Trim());
                    }
                }
            }

            return new StoreAndAuditDocumentActionResponse(correosNuevos);
        }
    }
}