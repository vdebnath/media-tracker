using MediaTracker.Data;
using MediaTracker.Data.Models;
using MediaTracker.Data.Security;
using MediaTracker.Data.Exceptions;
using MediaTracker.Services.Results;
using MediaTracker.Services.Enums;
using MediaTracker.Services.DTO;
using MediaTracker.Services.Extensions;

namespace MediaTracker.Services
{
    public class MediaItemService : IMediaItemService
    {
        private readonly IMediaItemRepository _mediaItemRepository;
        private readonly IMediaItemSecurityAuth _mediaItemSecurityAuth;

        public MediaItemService(IMediaItemRepository repository, IMediaItemSecurityAuth securityAuth)
        {
            _mediaItemRepository = repository;
            _mediaItemSecurityAuth = securityAuth;
        }

        public async Task<Result<List<MediaItemDTO>>> GetAllAsync()
        {
            List<MediaItem> mediaItems = await _mediaItemRepository.GetAllAsync();

            List<MediaItemDTO> mediaItemDTOs = mediaItems.Select(mediaItem => mediaItem.ModelToDTO()).ToList();

            return new Result<List<MediaItemDTO>>
            {
                Status = MediaItemResultStatus.Ok,
                Data = mediaItemDTOs,
                Message = "Items retrieved"
            };
        }

        public async Task<Result<MediaItemDTO?>> GetByIdAsync(int id)
        {
            Result<bool> itemExistenceCheck = await CheckItemExists(id);

            if (itemExistenceCheck.Data == false)
            {
                return new Result<MediaItemDTO?>
                {
                    Status = MediaItemResultStatus.NotFound,
                    Data = null,
                    Message = "Item could not be found"
                };
            }

            MediaItem? mediaItem = await _mediaItemRepository.GetByIdAsync(id);

            MediaItemDTO mediaItemDTO = mediaItem.ModelToDTO();

            return new Result<MediaItemDTO?>
            {
                Status = MediaItemResultStatus.Ok,
                Data = mediaItemDTO,
                Message = "Item retrieved"
            };
        }

        public async Task<Result<MediaItemDTO>> AddItemAsync(MediaItemDTO mediaItemDTO)
        {
            MediaItem mediaItem;

            try
            {
                mediaItem = mediaItemDTO.DtoToModel();
            }
            catch (InvalidMediaItemException ex)
            {
                return new Result<MediaItemDTO>
                {
                    Status = MediaItemResultStatus.ValidationFailed,
                    Data = null,
                    Message = ex.Message
                };
            }

            Result<bool> validationCheck = await CheckIsItemValid(mediaItem);

            if (validationCheck.Data == false)
            {
                return new Result<MediaItemDTO>
                {
                    Status = MediaItemResultStatus.ValidationFailed,
                    Data = null,
                    Message = "Validation Failed."
                };
            }

            MediaItem addedMediaItem = await _mediaItemRepository.AddItemAsync(mediaItem);
            MediaItemDTO addedMediaItemDTO = addedMediaItem.ModelToDTO();

            return new Result<MediaItemDTO>
            {
                Status = MediaItemResultStatus.CreatedAtAction,
                Data = addedMediaItemDTO,
                Message = "Item added successfully"
            };
            
        }

        public async Task<Result<bool>> UpdateItemAsync(MediaItemDTO updatedMediaItemDTO)
        {
            MediaItem mediaItem;

            try
            {
                mediaItem = updatedMediaItemDTO.DtoToModel();
            }
            catch (InvalidMediaItemException ex)
            {
                return new Result<bool>
                {
                    Status = MediaItemResultStatus.ValidationFailed,
                    Data = false,
                    Message = ex.Message
                };
            }

            Result<bool> validationCheck = await CheckIsItemValid(mediaItem);

            if (validationCheck.Data == false)
            {
                return new Result<bool>
                {
                    Status = MediaItemResultStatus.ValidationFailed,
                    Data = false,
                    Message = "Validation Failed."
                };
            }

            Result<bool> itemExistenceCheck = await CheckItemExists(mediaItem.Id);

            if (itemExistenceCheck.Data == true)
            {
                await _mediaItemRepository.UpdateItemAsync(mediaItem);

                return new Result<bool>
                {
                    Status = MediaItemResultStatus.NoContent,
                    Data = true,
                    Message = "Item updated successfully"
                };
            }

            return itemExistenceCheck;
        }

        public async Task<Result<bool>> DeleteItemAsync(int mediaItemId)
        {
            Result<bool> itemExistenceCheck = await CheckItemExists(mediaItemId);

            if (itemExistenceCheck.Data == true)
            {
                await _mediaItemRepository.DeleteItemAsync(mediaItemId);

                return new Result<bool>
                {
                    Status = MediaItemResultStatus.NoContent,
                    Data = true,
                    Message = "Item deleted successfully"
                };
            }

            return itemExistenceCheck;
        }

        private async Task<Result<bool>> CheckItemExists(int mediaItemId)
        {
            bool doesItemExist = await _mediaItemSecurityAuth.DoesMediaItemExist(mediaItemId);

            if (doesItemExist)
            {
                return new Result<bool>
                {
                    Status = MediaItemResultStatus.NoContent,
                    Data = true,
                    Message = "Item exists"
                };
            }

            return new Result<bool>
            {
                Status = MediaItemResultStatus.NotFound,
                Data = false,
                Message = "Item does not exist"
            };
        }

        private async Task<Result<bool>> CheckIsItemValid(MediaItem mediaItem)
        {
            try
            {
                await _mediaItemSecurityAuth.IsMediaItemValidAsync(mediaItem);
            }
            catch (InvalidMediaItemException ex)
            {
                return new Result<bool>
                {
                    Status = MediaItemResultStatus.ValidationFailed,
                    Data = false,
                    Message = ex.Message
                };
            }

            return new Result<bool>
            {
                Status = MediaItemResultStatus.Ok,
                Data = true,
                Message = "Validation passed"
            };

        }
    }
}