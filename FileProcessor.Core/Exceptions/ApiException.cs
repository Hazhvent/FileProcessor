namespace FileProcessor.Core.Exceptions;

public class ApiException : Exception
{
    public ApiError ErrorResponse { get; }

    // Constructor que arma el error (ApiError) listo para ser entregado al Middleware
    public ApiException(int statusCode, string? detail = null, List<ApiSubError>? subErrors = null) : base(detail)
    {
        // out var message: exporta el mensaje extraido de "HttpErrors" (diccionario de errores)
        // Si no se encuentra el statusCode proporcionado, entonces se asume error de sistema (500) y se extrae su mensaje correspondiente
        // Si se encuentra el statusCode proporcionado, entonces se asigna su mensaje correspondiente 
        var errorInfo = HttpErrorsCompendium.Map.TryGetValue(statusCode, out var message)
                        ? message
                        : HttpErrorsCompendium.Map[500];

        // Armado del error
        ErrorResponse = new ApiError
        (
            statusCode,
            errorInfo.Name,
            detail ?? errorInfo.Detail,
            subErrors
        );
    }
}

public sealed record ApiError(
    int statusCode,
    string name,
    string detail,
    List<ApiSubError>? subErrors = null
);
public sealed record ApiSubError(string Field, string Message);
