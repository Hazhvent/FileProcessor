using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Database;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Models.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FileProcessor.Infrastructure.Services;

public sealed class EmailService() : IEmailService
{
    public async Task SendEmail(EmailMessage message, IEnumerable<Parametros> smtpConfigurations)
    {
        // 1. Extraemos y mapeamos los datos técnicos desde la colección externa
        MailData smtpConfig = ExtractMailData(smtpConfigurations);
        var email = BuildMail(smtpConfig, message);

        using var client = new SmtpClient();
        try
        {
            // 🎯 Cambiado a Auto: La manera más segura y flexible de negociar TLS/SSL según el puerto
            await client.ConnectAsync(smtpConfig.Host, smtpConfig.Port, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(smtpConfig.Username, smtpConfig.Password);
            await client.SendAsync(email);
        }
        finally
        {
            // 🛡️ Blindaje defensivo: Evitamos excepciones secundarias al desconectar
            if (client.IsConnected)
            {
                await client.DisconnectAsync(quit: true);
            }
        }
    }

    // Convierte la colección en el modelo interno de configuración
    private static MailData ExtractMailData(IEnumerable<Parametros> smtpConfigurations)
    {
        if (smtpConfigurations == null || !smtpConfigurations.Any())
        {
            throw new Exceptions.InternalServerErrorException(ErrorConstants.INTERNAL_SMTP_CONFIG_EMPTY);
        }

        var smtpMap = smtpConfigurations.ToDictionary(p => p.Clave, p => p.Parametro);

        string usr = smtpMap.GetValueOrDefault(ParametersConstants.SMTP_USR) ?? string.Empty;
        string pwd = smtpMap.GetValueOrDefault(ParametersConstants.SMTP_PWD) ?? string.Empty;
        string host = smtpMap.GetValueOrDefault(ParametersConstants.SMTP_HOST) ?? string.Empty;
        string portStr = smtpMap.GetValueOrDefault(ParametersConstants.SMTP_PORT) ?? string.Empty;

        // 🎯 Alineado a tus excepciones personalizadas (HTTP 500)
        if (!int.TryParse(portStr, out int port))
        {
            throw new Exceptions.InternalServerErrorException(
                string.Format(ErrorConstants.INTERNAL_SMTP_PORT_INVALID, portStr));
        }

        return new MailData
        {
            Username = usr,
            Password = pwd,
            Host = host,
            Port = port
        };
    }

    // Construye el mensaje a enviar
    private static MimeMessage BuildMail(MailData config, EmailMessage message)
    {
        var bodyBuilder = new BodyBuilder { TextBody = message.Body };
        AddAttachedFile(bodyBuilder, message);

        var email = new MimeMessage
        {
            Subject = message.Subject,
            From = { new MailboxAddress(SettingsConstants.EMAIL_SENDER_DISPLAY_NAME, config.Username) },
            To = { MailboxAddress.Parse(message.Destinatary) },
            Body = bodyBuilder.ToMessageBody()
        };

        return email;
    }

    // Valida e inyecta el archivo adjunto al cuerpo del mensaje de forma segura
    private static void AddAttachedFile(BodyBuilder builder, EmailMessage message)
    {
        // Verifica defensivamente que existan tanto el flujo del archivo como su nombre
        if (message.Attachment != null && !string.IsNullOrWhiteSpace(message.AttachmentName))
        {
            // Rebobina el puntero al inicio del flujo si es navegable para evitar adjuntos vacíos (0 bytes)
            if (message.Attachment.CanSeek)
            {
                message.Attachment.Position = 0;
            }

            // Registra el archivo en el árbol MIME delegando a MimeKit el descarte automático del Content-Type
            builder.Attachments.Add(message.AttachmentName, message.Attachment);
        }
    }
}