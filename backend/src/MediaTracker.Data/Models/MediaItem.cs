namespace MediaTracker.Data.Models
{
    /// <summary>
    /// Different media types
    /// </summary>
    public enum MediaType
    {
        Show,
        Movie,
        Book
    }

    /// <summary>
    /// Correlates to status in UI
    /// </summary>
    public enum MediaStatus
    {
        Backlog,
        InProgress,
        Completed
    }

    /// <summary>
    /// Media Item definition
    /// </summary>
    public class MediaItem
    {
        public int Id {get; set;}
        public required string Title {get; set;}
        public MediaType Type {get; set;}
        public MediaStatus Status {get; set;}
        public string? Notes {get; set;}
        public DateTime DateAdded {get; set;}
    }
}