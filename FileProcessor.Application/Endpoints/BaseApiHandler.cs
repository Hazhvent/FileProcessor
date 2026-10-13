using MediatR;
using Microsoft.AspNetCore.Http;

namespace FileProcessor.Application.Endpoints
{
    public abstract class BaseApiHandler
    {
        // Método genérico para enviar una petición a MediatR
        protected static async Task<IResult> SendRequest<TRequest, TResponse>(
            ISender sender,
            TRequest request,
            CancellationToken cancellationToken
        ) where TRequest : IRequest<TResponse>
        {
            var result = await sender.Send(request, cancellationToken);
            return TypedResults.Ok(result);
        }
    }
}