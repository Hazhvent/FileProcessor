namespace FileProcessor.Core.Constants.Api;

/// <summary>
/// Valores de configuración para la documentación de la API.
/// </summary>
public static class SwaggerConstants
{
    // 📄 Metadatos OpenAPI
    public const string ApiTitle = "FileProcessor";
    public const string ApiVersion = "v1.0.0";
    public const string ApiDescription = "Api que procesa documentos entrantes al flujo de póliza electrónica.";

    // 👤 Fichas destacadas en formato Markdown
    public const string ApiAuthor = "> 👤 **Autor:** Creado y diseñado por Cesar Galvez.";
    public const string ApiContact = "> 📧 **Contacto:** Para consultas concretas utilice [hazhvent@hotmail.com](mailto:hazhvent@hotmail.com).";

    // 🖥️ Configuración de Swagger UI
    public const int UiDefaultModelsExpandDepth = -1;
    public const string UiSwaggerEndpointUrlTemplate = "../swagger/{0}/swagger.json";
    public const string UiRoutePrefix = "docs";

    // 🔌 Plantilla para la descripción enriquecida de la API
    public const string ApiDescriptionTemplate =
        "{0}\n\n{1}\n\n{2}\n\n---\n\n### 🔌 Endpoint SOAP (Legacy)\nEl contrato WSDL para sistemas externos se encuentra expuesto en: **[/{3}?wsdl](../{3}?wsdl)**";
}