using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Interfaces.Repositories;
using MediatR;

namespace FileProcessor.Application.Actions
{
    public sealed record RegisterBatchUsersActionRequest(
        List<Usuario> Usuarios,
        string LoteGenerado,
        Dictionary<string, string> ClavesPorDocumento
    ) : IRequest<RegisterBatchUsersActionResponse>;

    public sealed record RegisterBatchUsersActionResponse(bool Exitoso);

    public class RegisterBatchUsersActionHandler(IRepository repository)
        : IRequestHandler<RegisterBatchUsersActionRequest, RegisterBatchUsersActionResponse>
    {
        public async Task<RegisterBatchUsersActionResponse> Handle(RegisterBatchUsersActionRequest request, CancellationToken cancellationToken)
        {
            if (request.Usuarios == null || request.Usuarios.Count == 0)
            {
                return new RegisterBatchUsersActionResponse(true);
            }

            foreach (var usuario in request.Usuarios)
            {
                if (usuario.Documentos == null || usuario.Documentos.Count == 0) continue;

                var docsDelUsuario = usuario.Documentos
                    .Select(d => string.Format(SettingsConstants.PDF_FILE_NAME_TEMPLATE, d.Nombre.Trim()))
                    .ToList();

                string documentosConcatenados = string.Join(SettingsConstants.DELIMITER_SEMICOLON, docsDelUsuario) + SettingsConstants.DELIMITER_SEMICOLON;

                string nombrePrimerDocOriginal = usuario.Documentos.First().Nombre.Trim();
                if (request.ClavesPorDocumento.TryGetValue(nombrePrimerDocOriginal, out string? claveAsignada))
                {
                    usuario.DNI = claveAsignada;
                }

                await repository.InsertUsuariosAsync(request.LoteGenerado, usuario, documentosConcatenados);
            }

            return new RegisterBatchUsersActionResponse(true);
        }
    }
}