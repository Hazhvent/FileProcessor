namespace FileProcessor.Core.Models.Settings
{
    public sealed record PriorityWordsSettings
    {
        public List<string> Keys { get; init; } = [];
    }
}