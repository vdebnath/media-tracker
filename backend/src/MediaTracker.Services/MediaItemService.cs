using MediaTracker.Data;
using MediaTracker.Data.Models;

namespace MediaTracker.Services
{
    public class MediaItemService : IMediaItemService
    {
        private readonly IMediaItemRepository _mediaItemRepository;

        public MediaItemService(IMediaItemRepository repository)
        {
            _mediaItemRepository = repository;
        }

        public async Task<List<MediaItem>> GetAllAsync()
        {
            return await _mediaItemRepository.GetAllAsync();
        }

        public async Task<MediaItem?> GetByIdAsync(int id)
        {
            return await _mediaItemRepository.GetByIdAsync(id);
        }

        public async Task<MediaItem> AddItemAsync(MediaItem mediaItem)
        {
            return await _mediaItemRepository.AddItemAsync(mediaItem);
        }

        public async Task<MediaItem> UpdateItemAsync(MediaItem updatedMediaItem)
        {
            return await _mediaItemRepository.UpdateItemAsync(updatedMediaItem);
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            return await _mediaItemRepository.DeleteItemAsync(id);
        }
    }
}