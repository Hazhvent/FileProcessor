using FileProcessor.Core.Constants.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace FileProcessor.Application.Configuration;

public static class SwaggerConfiguration
{
    public static TBuilder AddSwagger<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        // 1. Obtenemos la ruta SOAP y le quitamos la barra inicial si la tiene
        string rawSoapPath = builder.Configuration[SettingsConstants.SOAP_SETTINGS_ENDPOINT_PATH_KEY] ?? string.Empty;
        string cleanSoapPath = rawSoapPath.TrimStart(SettingsConstants.PATH_SEPARATOR_SLASH);

        // 2. Formateamos la descripción Markdown con las tarjetas y el SOAP
        string enrichedDescription = string.Format(
            SwaggerConstants.ApiDescriptionTemplate,
            SwaggerConstants.ApiDescription,
            SwaggerConstants.ApiAuthor,
            SwaggerConstants.ApiContact,
            cleanSoapPath
        );

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(SwaggerConstants.ApiVersion, new OpenApiInfo
            {
                Title = SwaggerConstants.ApiTitle,
                Version = SwaggerConstants.ApiVersion,
                Description = enrichedDescription
            });
        });

        return builder;
    }

    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = SwaggerConstants.ApiTitle;
            options.EnableFilter();
            options.DefaultModelsExpandDepth(SwaggerConstants.UiDefaultModelsExpandDepth);
            options.DisplayRequestDuration();

            string swaggerUrl = string.Format(SwaggerConstants.UiSwaggerEndpointUrlTemplate, SwaggerConstants.ApiVersion);
            options.SwaggerEndpoint(swaggerUrl, SwaggerConstants.ApiTitle);
            options.EnableDeepLinking();
            options.RoutePrefix = SwaggerConstants.UiRoutePrefix;
        });

        return app;
    }
}