namespace FileProcessor.Core.Models.Settings
{
    // [no tocar 🔒]
    public sealed record ApplicationSettings // uso de null! por principio de fail-fast
    {
        public string LogDirectoryPath { get; init; } = null!;

    }
}