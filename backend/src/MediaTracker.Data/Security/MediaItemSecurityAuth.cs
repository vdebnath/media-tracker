using Microsoft.EntityFrameworkCore;
using MediaTracker.Data.Models;
using MediaTracker.Data.Exceptions;

namespace MediaTracker.Data.Security
{
    public class MediaItemSecurityAuth : IMediaItemSecurityAuth
    {
        private readonly MediaTrackerDbContext _mediaTrackerDbContext;
        
        public MediaItemSecurityAuth(MediaTrackerDbContext dbContext)
        {
            _mediaTrackerDbContext = dbContext;
        }

        public async Task<bool> IsMediaItemValidAsync(MediaItem mediaItem)
        {
            if (mediaItem == null)
            {
                throw new InvalidMediaItemException("MediaItem cannot be null");
            }

            if (mediaItem.Id < 0)
            {
                throw new InvalidMediaItemException("MediaItem has an invalid Id");
            }

            if (string.IsNullOrEmpty(mediaItem.Title))
            {
                throw new InvalidMediaItemException("Title is required");
            }

            return true;
        }

        public async Task<bool> DoesMediaItemExist(int mediaItemId)
        {
            MediaItem? existingMediaItem = await _mediaTrackerDbContext.MediaItems
                    .FirstOrDefaultAsync(item => item.Id == mediaItemId);

            if (existingMediaItem is null)
            {
                return false;
            }

            return true;
        }

    }
}