using FileProcessor.Application.UseCases;
using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using MediatR;
using System.Text.Json;

namespace FileProcessor.Application.Soap
{
    public class WsGenerarPDFexterno(ISender sender) : IWsGenerarPDFexterno
    {
        private readonly ISender _sender = sender;

        public string CargaDocumentoExterno(string informacionDocumentos, string informacionUsuarios, string rutaDirectorio)
        {
            try
            {
                // 1. DESERIALIZAR
                var opcionesJson = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };

                var documentos = JsonSerializer.Deserialize<List<Documento>>(informacionDocumentos, opcionesJson)
                                 ?? [];

                var usuarios = JsonSerializer.Deserialize<List<Usuario>>(informacionUsuarios, opcionesJson)
                               ?? [];

                // 2. ARMAR REQUEST
                var request = new ProcessDocumentBatchRequest(
                    Documentos: documentos,
                    Usuarios: usuarios,
                    DirectoryPath: rutaDirectorio
                );

                // 3. ENVIAR
                var response = _sender.Send(request).GetAwaiter().GetResult();

                // 4. RESPUESTA EXITOSA
                var respuestaExitosa = new
                {
                    Estatus = SettingsConstants.SOAP_RESPONSE_STATUS_OK,
                    Error = string.Empty
                };

                return JsonSerializer.Serialize(respuestaExitosa);
            }
            catch (ApiException apiEx)
            {
                // 🔥 ATRAPAMOS ERRORES CONTROLADOS Y LOS MAPEAMOS CON CONSTANTES
                string errorMessage = apiEx.ErrorResponse.detail;

                if (apiEx.ErrorResponse.subErrors != null && apiEx.ErrorResponse.subErrors.Count > 0)
                {
                    // Armamos la lista: "[Campo]: Error del campo"
                    var erroresDetallados = apiEx.ErrorResponse.subErrors
                        .Select(e => string.Format(ErrorConstants.SOAP_VALIDATION_FIELD_ERROR, e.Field, e.Message));

                    // Unimos la lista con el separador " | "
                    string detalleUnido = string.Join(ErrorConstants.SOAP_VALIDATION_SEPARATOR, erroresDetallados);

                    // Fusionamos: "Error principal Detalle: [Campo]: Error..."
                    errorMessage = string.Format(ErrorConstants.SOAP_VALIDATION_ERROR_DETAIL, errorMessage, detalleUnido);
                }

                var respuestaError = new
                {
                    Estatus = SettingsConstants.SOAP_RESPONSE_STATUS_ERROR,
                    Error = errorMessage
                };

                return JsonSerializer.Serialize(respuestaError);
            }
            catch (Exception ex)
            {
                // 🔥 ATRAPAMOS ERRORES FATALES
                var respuestaError = new
                {
                    Estatus = SettingsConstants.SOAP_RESPONSE_STATUS_ERROR,
                    Error = string.Format(ErrorConstants.INTERNAL_SOAP_FATAL_ERROR, ex.ToString())
                };

                return JsonSerializer.Serialize(respuestaError);
            }
        }
    }
}