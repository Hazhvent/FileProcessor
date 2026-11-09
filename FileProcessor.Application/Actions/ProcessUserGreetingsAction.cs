using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Repositories;
using MediatR;

namespace FileProcessor.Application.Actions
{
    public sealed record ProcessUserGreetingsActionRequest(
        List<Usuario> Usuarios
    ) : IRequest<ProcessUserGreetingsActionResponse>;

    public sealed record ProcessUserGreetingsActionResponse(bool Exitoso);

    public class ProcessUserGreetingsActionHandler(IRepository repository)
        : IRequestHandler<ProcessUserGreetingsActionRequest, ProcessUserGreetingsActionResponse>
    {
        public async Task<ProcessUserGreetingsActionResponse> Handle(ProcessUserGreetingsActionRequest request, CancellationToken cancellationToken)
        {
            if (request.Usuarios == null || request.Usuarios.Count == 0)
            {
                return new ProcessUserGreetingsActionResponse(true);
            }

            var businessErrors = new List<ApiSubError>();

            foreach (var usuario in request.Usuarios)
            {
                var destinatario = await repository.GetDataDestinatarioAsync(usuario.DNI);
                if (destinatario == null)
                {
                    businessErrors.Add(new ApiSubError(ValidationConstants.PROPERTY_DNI, string.Format(ErrorConstants.REQUEST_DESTINATARIO_NOT_FOUND, usuario.DNI)));
                }
                else
                {
                    if (destinatario.SSEXCLIEN == ValidationConstants.GENDER_MALE)
                        usuario.Nombre = string.Format(ValidationConstants.GREETING_FORMAT_MALE, usuario.Nombre);
                    else if (destinatario.SSEXCLIEN == ValidationConstants.GENDER_FEMALE)
                        usuario.Nombre = string.Format(ValidationConstants.GREETING_FORMAT_FEMALE, usuario.Nombre);
                }
            }

            if (businessErrors.Count != 0)
            {
                throw new Exceptions.UnprocessableEntityException(businessErrors);
            }

            return new ProcessUserGreetingsActionResponse(true);
        }
    }
}