using FileProcessor.Application.Extensions;
using FileProcessor.Application.Pipeline.Middlewares;
using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Models.Settings;
using FileProcessor.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalApis.Discovery;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileProcessor.Application.Configuration
{
    public static class ApiConfiguration
    {
        public static TBuilder AddApi<TBuilder>(this TBuilder builder)
                where TBuilder : IHostApplicationBuilder
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            AddServices(builder.Services);
            AddSettings(builder.Services, builder.Configuration);

            return builder;
        }

        public static void UseApiConfiguration(this WebApplication app)
        {
            app.AddMiddlewares();
            app.MapApis(); // Registro automático de todos los endpoints que implementan IApi
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddEndpointsApiExplorer(); // Hace visible las Minimal APIs en el Swagger 
            services.AddHttpContextAccessor(); // Permite obtener información sobre la solicitud actual recibida

            // Servicios personalizados: 
            services.AddSingleton(typeof(ILoggingService<>), typeof(LoggingService<>));
            services.AddSingleton<IHttpErrorFormatter, HttpErrorFormatter>();
            services.AddSingleton<INetworkConnectionService, NetworkConnectionService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddScoped<IPdfProcessorService, PdfProcessorService>();
            services.AddScoped<IEmailService, EmailService>();
        }

        private static void AddSettings(IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de serialización JSON
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            // Configuraciones desde appsettings.json
            services.Configure<ApplicationSettings>(configuration.GetSection(SettingsConstants.APPLICATION_SETTINGS_SECTION_NAME));
            services.Configure<NASSettings>(configuration.GetSection(SettingsConstants.NAS_SETTINGS_SECTION_NAME));
            services.Configure<PriorityWordsSettings>(configuration.GetSection(SettingsConstants.PRIORITY_WORDS_SECTION_NAME));
        }

        public static void AddMiddlewares(this IApplicationBuilder app)
        {
            app.UseHttpsRedirection(); // Redirección automática a HTTPS (Por si el cliente usa HTTP)
            app.UseStatusCodePagesHandler(); // Maneja códigos de estado HTTP que no llegan al pipeline de la aplicacion (extension)

            // Middlewares personalizados:
            app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}