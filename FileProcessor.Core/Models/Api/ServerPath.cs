namespace FileProcessor.Core.Models.Api
{
    public sealed class ServerPath
    {
        public string Host { get; set; } = string.Empty;
        public string ShareFolder { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
    }
}