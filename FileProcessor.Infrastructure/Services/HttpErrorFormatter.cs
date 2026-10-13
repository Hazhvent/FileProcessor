using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FileProcessor.Infrastructure.Services
{
    // [no tocar 🔒]
    public class HttpErrorFormatter(
        IOptions<JsonOptions> jsonOptions) : IHttpErrorFormatter
    {
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions;

        public async Task WriteErrorResponseAsync(
            HttpContext context,
            ApiException exception)
        {
            context.Response.ContentType = SettingsConstants.HTTP_CONTENT_TYPE_JSON;
            context.Response.StatusCode = exception.ErrorResponse.statusCode;

            var json = JsonSerializer.Serialize(exception.ErrorResponse, _jsonOptions);
            context.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(json);

            await context.Response.WriteAsync(json);
        }
    }
}