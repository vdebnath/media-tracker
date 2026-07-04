using MediaTracker.Data.Models;

namespace MediaTracker.Services
{
    public interface IMediaItemService
    {
        Task<List<MediaItem>> GetAllAsync();
        Task<MediaItem?> GetByIdAsync(int id);
        Task<MediaItem> AddItemAsync(MediaItem mediaItem);
        Task<bool> UpdateItemAsync(MediaItem mediaItem);
        Task<bool> DeleteItemAsync(int id);
    }
}