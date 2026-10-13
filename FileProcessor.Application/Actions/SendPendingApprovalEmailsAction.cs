using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Interfaces.Repositories;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Models.Email;
using MediatR;

namespace FileProcessor.Application.Actions
{
    public sealed record SendPendingApprovalEmailsActionRequest(
        List<string> Correos,
        string LoteGenerado
    ) : IRequest<SendPendingApprovalEmailsActionResponse>;

    public sealed record SendPendingApprovalEmailsActionResponse(bool Exitoso);

    public class SendPendingApprovalEmailsActionHandler(
        IRepository repository,
        IEmailService emailService)
        : IRequestHandler<SendPendingApprovalEmailsActionRequest, SendPendingApprovalEmailsActionResponse>
    {
        public async Task<SendPendingApprovalEmailsActionResponse> Handle(SendPendingApprovalEmailsActionRequest request, CancellationToken cancellationToken)
        {
            var correosUnicos = request.Correos
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .Distinct()
                .ToList();

            if (correosUnicos.Count == 0)
            {
                return new SendPendingApprovalEmailsActionResponse(true);
            }

            var smtpConfig = await repository.GetMailDataAsync();

            foreach (var correo in correosUnicos)
            {
                var mensaje = new EmailMessage
                {
                    Destinatary = correo,
                    Subject = SettingsConstants.EMAIL_SUBJECT_PENDING_APPROVAL,
                    Body = string.Format(SettingsConstants.EMAIL_BODY_PENDING_APPROVAL_TEMPLATE, request.LoteGenerado),
                    Attachment = null,
                    AttachmentName = string.Empty
                };

                await emailService.SendEmail(mensaje, smtpConfig);
            }

            return new SendPendingApprovalEmailsActionResponse(true);
        }
    }
}