using MediaTracker.Data.Models;

namespace MediaTracker.Data
{
    /// <summary>
    /// Interface to define all CRUD operations used in Data project 
    /// </summary>
    public interface IMediaItemRepository
    {
        /// <summary>
        /// Retrieves list of all media items
        /// </summary>
        /// <returns>List of all media items</returns>
        /// <remarks>Added AsNoTracking to keep list read only</remarks>
        Task<List<MediaItem>> GetAllAsync();

        /// <summary>
        /// Retrieves one media item from DB based on ID
        /// </summary>
        /// <param name="id">ID to identify media item</param>
        /// <returns>Media item with matching ID</returns>
        /// <remarks>
        /// Added AsNoTracking to keep item read only
        /// Made return nullable in case id does not exist
        /// </remarks>
        Task<MediaItem?> GetByIdAsync(int id);

        /// <summary>
        /// Insert a media item into the db
        /// </summary>
        /// <param name="mediaItem">Item to add</param>
        /// <returns>Added item</returns>
        Task<MediaItem> AddItemAsync(MediaItem mediaItem);

        /// <summary>
        /// Edits an existing media item
        /// </summary>
        /// <param name="updatedMediaItem">Media item to update</param>
        /// <returns>True if updated, false if item does not exist</returns>
        /// <remarks>
        /// Load the tracked item first
        /// Copy scalar values with SetValues 
        /// Updates only changed columns
        /// </remarks>
        Task<bool> UpdateItemAsync(MediaItem mediaItem);

        /// <summary>
        /// Removes a media item
        /// </summary>
        /// <param name="id">ID of item to delete</param>
        /// <returns>True if deleted, false if item does not exist</returns>
        Task<bool> DeleteItemAsync(int id);
    }
}