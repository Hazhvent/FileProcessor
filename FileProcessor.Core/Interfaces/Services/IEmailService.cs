using FileProcessor.Core.Dto;
using FileProcessor.Core.Models.Email;

namespace FileProcessor.Core.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmail(EmailMessage message, IEnumerable<Parametros> smtpConfigurations);
    }
}
