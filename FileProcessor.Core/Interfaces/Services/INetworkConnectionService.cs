using SMBLibrary.Client;

namespace FileProcessor.Core.Interfaces.Services;

public interface INetworkConnectionService
{
    // Método para la lectura dinámica (Request XML)
    Task RunInNetworkScope(string hostName, string shareName, Action<ISMBFileStore> action, int credentialMode = 1);

    // 🟢 NUEVO: Método para la infraestructura fija (Escribe / Crea sobre NAS base)
    Task RunInNetworkScope(Action<ISMBFileStore> action, int credentialMode = 1);
}