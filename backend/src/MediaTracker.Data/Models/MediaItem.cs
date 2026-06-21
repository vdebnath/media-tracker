namespace MediaTracker.Data.Models
{
    public enum MediaType
    {
        Show,
        Movie,
        Book
    }

    public enum MediaStatus
    {
        Backlog,
        InProgress,
        Completed
    }

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