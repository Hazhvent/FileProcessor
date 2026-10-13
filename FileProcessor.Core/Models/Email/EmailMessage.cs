namespace FileProcessor.Core.Models.Email
{
    public sealed record EmailMessage
    {
        public string Destinatary { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public Stream? Attachment { get; init; }
        public string? AttachmentName { get; init; }
    }
}