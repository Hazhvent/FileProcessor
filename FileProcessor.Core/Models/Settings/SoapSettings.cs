namespace FileProcessor.Core.Models.Settings
{
    public sealed record SoapSettings
    {
        public string EndpointPath { get; init; } = string.Empty;
    }
}