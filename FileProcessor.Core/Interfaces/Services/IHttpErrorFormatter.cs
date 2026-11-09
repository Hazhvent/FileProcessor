using FileProcessor.Core.Exceptions;
using Microsoft.AspNetCore.Http;

namespace FileProcessor.Core.Interfaces.Services
{
    public interface IHttpErrorFormatter
    {
        Task WriteErrorResponseAsync(HttpContext context, ApiException exception);
    }
}