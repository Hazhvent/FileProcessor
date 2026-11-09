using FileProcessor.Core.Dto;
using SMBLibrary.Client;

namespace FileProcessor.Core.Interfaces.Services;

public interface IStorageService
{
    // Valida y crea recursivamente toda la estructura de directorios directamente en el NAS (Red)
    void EnsureNetworkDirectoriesExist(ISMBFileStore fileStore, IEnumerable<Parametros> routeConfigurations);

    // Valida si un directorio de red existe usando el túnel SMB
    bool CheckNetworkDirectoryExists(ISMBFileStore fileStore, string relativePath);

    // Obtiene el tamaño exacto de un archivo en bytes a través del túnel SMB
    long? GetNetworkFileSize(ISMBFileStore fileStore, string filePath);

    // Lee el contenido de un archivo de texto desde el NAS (Red) directo a memoria (RAM) por SMB
    string ReadNetworkTextFile(ISMBFileStore fileStore, string smbFilePath);

    // 🌟 NUESTRO ÚNICO WRITE: Guarda un arreglo de bytes (RAM) físicamente en el NAS (Red) procesando por fragmentos
    void WriteNetworkFile(ISMBFileStore fileStore, string filePath, byte[] fileBytes);

    // Elimina de forma segura un archivo residual o temporal dentro del NAS (Red)
    void DeleteNetworkFile(ISMBFileStore fileStore, string filePath);
}