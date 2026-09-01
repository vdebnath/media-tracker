using Microsoft.EntityFrameworkCore;
using MediaTracker.Data.Models;

namespace MediaTracker.Data
{
    /// <summary>
    /// Implementation of IMediaItemRepository interface
    /// Defines all EF core logic for all CRUD operations 
    /// </summary>
    public class MediaItemRepository : IMediaItemRepository
    {
        private readonly MediaTrackerDbContext _mediaTrackerDbContext;

        /// <summary>
        /// Accepts the dbContext via injection
        /// </summary>
        /// <param name="dbContext">DbContext being used</param>
        public MediaItemRepository(MediaTrackerDbContext dbContext)
        {
            _mediaTrackerDbContext = dbContext;
        }

        public async Task<List<MediaItem>> GetAllAsync()
        {
            List<MediaItem> mediaItemsList = await _mediaTrackerDbContext.MediaItems.AsNoTracking().ToListAsync();
            return mediaItemsList;
        }
        
        public async Task<MediaItem?> GetByIdAsync(int id)
        {
            MediaItem? mediaItem = await _mediaTrackerDbContext.MediaItems
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            return mediaItem;
        }
        
        public async Task<MediaItem> AddItemAsync(MediaItem mediaItem)
        {
            await _mediaTrackerDbContext.AddAsync(mediaItem);
            await _mediaTrackerDbContext.SaveChangesAsync();

            return mediaItem;
        }

        public async Task<bool> UpdateItemAsync(MediaItem updatedMediaItem)
        {
            MediaItem? existingMediaItem = await _mediaTrackerDbContext.MediaItems
                .FirstOrDefaultAsync(item => item.Id == updatedMediaItem.Id);
                
            _mediaTrackerDbContext.Entry(existingMediaItem).CurrentValues.SetValues(updatedMediaItem);
            await _mediaTrackerDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            MediaItem? itemToDelete = await _mediaTrackerDbContext.MediaItems.FindAsync(id);
            _mediaTrackerDbContext.MediaItems.Remove(itemToDelete);
            await _mediaTrackerDbContext.SaveChangesAsync();

            return true;
        }
    }
}