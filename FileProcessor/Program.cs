using FileProcessor.Application.Configuration;
using FileProcessor.Infrastructure.Persistence;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Extensiones de builder
        builder.AddApi();
        builder.AddLogs();
        builder.AddSwagger();
        builder.AddMediatR();
        builder.AddDataBase();
        builder.AddSoap();

        var app = builder.Build();
        // Extensiones de app       
        app.UseApiConfiguration();
        app.UseSwaggerConfiguration();
        app.UseSoapConfiguration();

        await app.RunAsync();
    }

}