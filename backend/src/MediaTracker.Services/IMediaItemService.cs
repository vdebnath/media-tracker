using MediaTracker.Services.Results;
using MediaTracker.Services.DTO;

namespace MediaTracker.Services
{
    public interface IMediaItemService
    {
        Task<Result<List<MediaItemDTO>>> GetAllAsync();
        Task<Result<MediaItemDTO?>> GetByIdAsync(int id);
        Task<Result<MediaItemDTO>> AddItemAsync(MediaItemDTO mediaItemDTO);
        Task<Result<bool>> UpdateItemAsync(MediaItemDTO mediaItemDTO);
        Task<Result<bool>> DeleteItemAsync(int mediaItemId);
    }
}