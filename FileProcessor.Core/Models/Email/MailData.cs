namespace FileProcessor.Core.Models.Email
{
    public sealed record MailData
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string Host { get; init; } = string.Empty;
        public int Port { get; init; }
    }
}
