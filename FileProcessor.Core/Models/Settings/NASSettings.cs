namespace FileProcessor.Core.Models.Settings
{
    // [no tocar 🔒]
    public sealed record NASSettings // uso de null! por principio de fail-fast
    {
        public string Host { get; init; } = null!;
        public string Domain { get; init; } = null!;
        public string NewUsername { get; init; } = null!;
        public string NewPassword { get; init; } = null!;
        public string OldUsername { get; init; } = null!;
        public string OldPassword { get; init; } = null!;
        public string ShareFolder { get; init; } = null!;
    }
}