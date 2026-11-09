using FileProcessor.Application.Soap;
using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Models.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoapCore;

namespace FileProcessor.Application.Configuration
{
    public static class SoapConfiguration
    {
        public static TBuilder AddSoap<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            builder.Services.Configure<SoapSettings>(
                builder.Configuration.GetSection(SettingsConstants.SOAP_SETTINGS_SECTION_NAME)
            );

            builder.Services.AddSoapCore();
            builder.Services.AddScoped<IWsGenerarPDFexterno, WsGenerarPDFexterno>();

            return builder;
        }

        public static void UseSoapConfiguration(this WebApplication app)
        {
            string? endpointPath = app.Configuration[SettingsConstants.SOAP_SETTINGS_ENDPOINT_PATH_KEY];

            if (string.IsNullOrWhiteSpace(endpointPath))
            {
                throw new InvalidOperationException(ErrorConstants.INTERNAL_NOT_FOUND_SOAP_ENDPOINT_PATH);
            }

            ((IApplicationBuilder)app).UseSoapEndpoint<IWsGenerarPDFexterno>(options =>
            {
                options.Path = endpointPath;
                options.SoapSerializer = SoapSerializer.DataContractSerializer;
                options.CaseInsensitivePath = true;
            });
        }
    }
}