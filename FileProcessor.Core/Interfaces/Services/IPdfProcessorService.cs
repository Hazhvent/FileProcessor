namespace FileProcessor.Core.Interfaces.Services;

public interface IPdfProcessorService
{
    // Convierte un flujo RTF y devuelve el flujo del PDF resultante en memoria
    Task<MemoryStream> ConvertRtfToPdfStream(Stream rtfInputStream);

    // Cifra un PDF leyendo un flujo de entrada y devolviendo uno nuevo protegido por contraseña
    MemoryStream EncryptPdfStream(Stream pdfInputStream, string password);
}