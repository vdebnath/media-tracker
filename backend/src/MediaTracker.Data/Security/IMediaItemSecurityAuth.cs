using MediaTracker.Data.Models;

namespace MediaTracker.Data.Security
{
    public interface IMediaItemSecurityAuth
    {
        Task<bool> IsMediaItemValidAsync(MediaItem mediaItem);
        Task<bool> DoesMediaItemExist(int mediaItemId);
    }
}