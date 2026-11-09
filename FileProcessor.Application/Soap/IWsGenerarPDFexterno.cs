using FileProcessor.Core.Constants.Api;
using System.ServiceModel;

namespace FileProcessor.Application.Soap
{
    [ServiceContract(Namespace = SettingsConstants.SOAP_DEFAULT_NAMESPACE)]
    public interface IWsGenerarPDFexterno
    {
        [OperationContract(Action = SettingsConstants.SOAP_ACTION_CARGA_DOCUMENTO)]
        string CargaDocumentoExterno(string informacionDocumentos, string informacionUsuarios, string rutaDirectorio);
    }
}