using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Repositories;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Models.Api;
using FileProcessor.Core.Utils;
using MediatR;

namespace FileProcessor.Application.Actions
{
    public sealed record PrepareNetworkDirectoriesActionRequest(
        string? DirectoryPath
    ) : IRequest<PrepareNetworkDirectoriesActionResponse>;

    public sealed record PrepareNetworkDirectoriesActionResponse(
        string RutaPendientesNasBase,
        string RutaPrioridadNasBase,
        ServerPath? ServerPathOrigen,
        List<Parametros> ConfiguracionesRutasApi
    );

    public class PrepareNetworkDirectoriesActionHandler(
        IRepository repository,
        INetworkConnectionService networkConnectionService,
        IStorageService storageService)
        : IRequestHandler<PrepareNetworkDirectoriesActionRequest, PrepareNetworkDirectoriesActionResponse>
    {
        private static readonly HashSet<string> NewUserNasKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            SettingsConstants.DB_PARAM_NAS_RUTA_PRIORIDAD,
            SettingsConstants.DB_PARAM_NAS_RUTA_NOE,
            SettingsConstants.DB_PARAM_NAS_RUTA_APR
        };

        public async Task<PrepareNetworkDirectoriesActionResponse> Handle(PrepareNetworkDirectoriesActionRequest request, CancellationToken cancellationToken)
        {
            var configuracionesRutasApi = (await repository.GetApiPathsDataAsync())?.ToList() ?? [];

            string rutaPendientesNasBase = Helper.GetRouteOrThrow(configuracionesRutasApi, SettingsConstants.DB_PARAM_NAS_RUTA_APR);
            string rutaPrioridadNasBase = Helper.GetRouteOrThrow(configuracionesRutasApi, SettingsConstants.DB_PARAM_NAS_RUTA_PRIORIDAD);

            ServerPath? serverPathOrigen = null;

            // 🔵 1. ÁMBITO DINÁMICO: Validar la ruta de origen enviada en la petición XML
            if (!string.IsNullOrWhiteSpace(request.DirectoryPath))
            {
                serverPathOrigen = Helper.ParseServerPath(request.DirectoryPath)
                    ?? throw new Exceptions.BadRequestException(string.Format(ErrorConstants.REQUEST_PATH_NOT_EXISTS, request.DirectoryPath));

                await networkConnectionService.RunInNetworkScope(serverPathOrigen.Host, serverPathOrigen.ShareFolder, (fileStore) =>
                {
                    if (!storageService.CheckNetworkDirectoryExists(fileStore, serverPathOrigen.RelativePath))
                    {
                        throw new Exceptions.BadRequestException(string.Format(ErrorConstants.REQUEST_PATH_NOT_EXISTS, serverPathOrigen.RelativePath));
                    }
                }, credentialMode: 0);
            }

            // 🟢 2. ÁMBITO FIJO: Extraer las rutas internas de la BD y crear carpetas faltantes en el NAS
            var rawRutasNasNuevo = configuracionesRutasApi
                .Where(r => !string.IsNullOrWhiteSpace(r.Parametro) && NewUserNasKeys.Contains(r.Clave))
                .Select(r => new Parametros
                {
                    Clave = r.Clave,
                    Parametro = Helper.ExtractInternalPath(r.Parametro)
                })
                .ToList();

            if (rawRutasNasNuevo.Count != 0)
            {
                await networkConnectionService.RunInNetworkScope((fileStore) =>
                {
                    storageService.EnsureNetworkDirectoriesExist(fileStore, rawRutasNasNuevo);
                }, credentialMode: 1);
            }

            return new PrepareNetworkDirectoriesActionResponse(
                RutaPendientesNasBase: rutaPendientesNasBase,
                RutaPrioridadNasBase: rutaPrioridadNasBase,
                ServerPathOrigen: serverPathOrigen,
                ConfiguracionesRutasApi: configuracionesRutasApi
            );
        }
    }
}