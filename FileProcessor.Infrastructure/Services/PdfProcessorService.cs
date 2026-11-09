using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Services;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using RtfPipe;

namespace FileProcessor.Infrastructure.Services;

public sealed class PdfProcessorService : IPdfProcessorService
{
    // =========================================================================
    // 1. CONVERSIÓN RTF A PDF (Flujos Puros en Memoria)
    // =========================================================================

    public async Task<MemoryStream> ConvertRtfToPdfStream(Stream rtfInputStream)
    {
        if (rtfInputStream == null || rtfInputStream.Length == 0)
        {
            throw new Exceptions.BadRequestException(ErrorConstants.REQUEST_JSON_SYNTAX_INVALID);
        }

        try
        {
            // 🟢 Reseteo de posición para evitar lectura de streams previamente consumidos
            if (rtfInputStream.CanSeek)
            {
                rtfInputStream.Position = 0;
            }

            // 🟢 leaveOpen: true previene que el StreamReader destruya el Stream original de entrada
            using var reader = new StreamReader(rtfInputStream, leaveOpen: true);
            string rtfContent = await reader.ReadToEndAsync();
            string htmlContent = Rtf.ToHtml(rtfContent);

            // 🟢 Envoltura explícita con charset UTF-8 usando constante para garantizar codificación correcta de tildes y ñ
            string fullHtmlContent = string.Format(SettingsConstants.PUPPETEER_HTML_WRAPPER_TEMPLATE, htmlContent);

            // 🟢 Construcción dinámica con constantes
            string chromiumPath = Path.Combine(
                AppContext.BaseDirectory,
                SettingsConstants.PUPPETEER_CHROMIUM_DIRECTORY,
                SettingsConstants.PUPPETEER_CHROMIUM_EXECUTABLE
            );

            if (!File.Exists(chromiumPath))
            {
                throw new Exceptions.NotFoundException(
                    string.Format(ErrorConstants.INTERNAL_PUPPETEER_EXECUTABLE_NOT_FOUND, chromiumPath)
                );
            }

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = chromiumPath,
                Args = [SettingsConstants.PUPPETEER_ARG_NO_SANDBOX, SettingsConstants.PUPPETEER_ARG_DISABLE_SETUID_SANDBOX]
            };

            await using var browser = await Puppeteer.LaunchAsync(launchOptions);
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(fullHtmlContent);

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = SettingsConstants.PUPPETEER_MARGIN_DEFAULT,
                    Bottom = SettingsConstants.PUPPETEER_MARGIN_DEFAULT,
                    Left = SettingsConstants.PUPPETEER_MARGIN_DEFAULT,
                    Right = SettingsConstants.PUPPETEER_MARGIN_DEFAULT
                }
            };

            byte[] pdfBytes = await page.PdfDataAsync(pdfOptions);
            return new MemoryStream(pdfBytes);
        }
        catch (ApiException)
        {
            throw; // 🟢 Propaga excepciones personalizadas de API (400, 404, etc.)
        }
        catch (Exception ex)
        {
            throw new Exceptions.InternalServerErrorException(ex.Message);
        }
    }

    // =========================================================================
    // 2. CIFRADO SIMÉTRICO PROTEGIDO (Flujos Puros en Memoria)
    // =========================================================================
    public MemoryStream EncryptPdfStream(Stream pdfInputStream, string password)
    {
        if (pdfInputStream == null || pdfInputStream.Length == 0)
        {
            throw new Exceptions.BadRequestException(ErrorConstants.REQUEST_JSON_SYNTAX_INVALID);
        }

        try
        {
            if (pdfInputStream.CanSeek)
            {
                pdfInputStream.Position = 0;
            }

            using PdfDocument document = PdfReader.Open(pdfInputStream, PdfDocumentOpenMode.Modify);

            // 👈 Asignar contraseñas (User y Owner con la misma clave del legacy)
            document.SecuritySettings.UserPassword = password;
            document.SecuritySettings.OwnerPassword = password;

            // 👈 Forzar el nivel 40 bits DESPUÉS de las contraseñas
            document.SecurityHandler.SetEncryption(PdfDefaultEncryption.V2With40Bits);

            // 👈 Permisos exactos requeridos por compatibilidad con PdfSharp
            document.SecuritySettings.PermitPrint = true;
            document.SecuritySettings.PermitModifyDocument = true;
            document.SecuritySettings.PermitAnnotations = true;
            document.SecuritySettings.PermitExtractContent = true;

            MemoryStream outputStream = new();
            document.Save(outputStream, false);
            outputStream.Position = 0;

            return outputStream;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exceptions.InternalServerErrorException(
                string.Format(ErrorConstants.INTERNAL_PDF_ENCRYPTION_FAILED_WITH_DETAIL, ex.Message)
            );
        }
    }
}