using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Interfaces.Repositories;
using FileProcessor.Core.Models.Settings;
using FileProcessor.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace FileProcessor.Infrastructure.Persistence
{
    public static class DatabaseConfiguration
    {
        public static TBuilder AddDataBase<TBuilder>(this TBuilder builder)
                where TBuilder : IHostApplicationBuilder
        {

            var applicationSettings = builder.Configuration
                .GetSection(SettingsConstants.APPLICATION_SETTINGS_SECTION_NAME)
                .Get<ApplicationSettings>();

            if (applicationSettings == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        ErrorConstants.INTERNAL_NOT_FOUND_CONFIG_SECTION,
                        SettingsConstants.APPLICATION_SETTINGS_SECTION_NAME));
            }

            string connectionString = builder.Configuration.GetConnectionString(SettingsConstants.DB_CONNECTION_STRING_NAME)!;

            // Registramos IDbConnection para Dapper
            // Usamos AddScoped para que la conexión se mantenga durante el ciclo de vida de la petición HTTP
            builder.Services.AddScoped<IDbConnection>(sp =>
            {
                var connection = new OracleConnection(connectionString);
                return connection;
            });

            AddRepositories(builder.Services);
            return builder;
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IRepository, Repository>();
        }
    }
}