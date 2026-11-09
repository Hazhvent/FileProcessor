namespace FileProcessor.Core.Exceptions;

public static class HttpErrorsCompendium // [no tocar 🔒] 
{
    public sealed record ErrorInfo(string Name, string Detail);

    public static readonly Dictionary<int, ErrorInfo> Map = new()
    {
        /*✅*/
        [400] = new("Bad Request", "Error de sintaxis en el formato de solicitud."),
        /*✅*/
        [401] = new("Unauthorized", "Error de Autenticación: Por favor, inicie sesión para continuar."),
        /*✅*/
        [403] = new("Forbidden", "Error de Autorización: Usted no tiene permisos suficientes para acceder a este recurso."),
        /*✅*/
        [404] = new("Not Found", "El recurso solicitado no existe."),
        /*✅*/
        [405] = new("Method Not Allowed", "El método HTTP para este recurso es incorrecto."),
        /*✅*/
        [409] = new("Conflict", "Ha ocurrido un conflicto con la solicitud."),
        /*✅*/
        [413] = new("Payload Too Large", "La solicitud contiene al menos un archivo que excede el límite del tamaño permitido."),
        /*✅*/
        [415] = new("Unsupported Media Type", "La solicitud contiene al menos un elemento que no es compatible con el formato esperado."),
        /*✅*/
        [422] = new("Unprocessable Entity", "Error de validación: los datos enviados son incorrectos."),
        /*✅*/
        [429] = new("Too Many Requests", "Has realizado demasiadas solicitudes en poco tiempo. Por favor, inténtalo más tarde."),
        /*✅*/
        [500] = new("Internal Server Error", "Ha ocurrido un error inesperado en el servidor."),
        /*✅*/
        [503] = new("Service Unavailable", "Ha ocurrido un error inesperado en el servidor externo que atiende esta solicitud."),
    };
}
