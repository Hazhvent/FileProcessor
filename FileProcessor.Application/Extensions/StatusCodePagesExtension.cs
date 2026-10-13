using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FileProcessor.Application.Extensions
{
    public static class StatusCodePagesExtension
    {
        public static IApplicationBuilder UseStatusCodePagesHandler(this IApplicationBuilder app)
        {
            app.UseStatusCodePages(async context =>
            {
                var statusCode = context.HttpContext.Response.StatusCode;
                var errorResponseWriter = context.HttpContext.RequestServices.GetRequiredService<IHttpErrorFormatter>();

                if (HttpErrorsCompendium.Map.ContainsKey(statusCode))
                {
                    ApiException apiEx = new ApiException(statusCode);
                    await errorResponseWriter.WriteErrorResponseAsync(context.HttpContext, apiEx);
                }
            });

            return app;
        }
    }
}
