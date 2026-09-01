using MediaTracker.Data.Models;
using MediaTracker.Data.Exceptions;
using MediaTracker.Services.DTO;

namespace MediaTracker.Services.Extensions
{
    public static class MediaItemExtensions
    {
        public static MediaItemDTO ModelToDTO(this MediaItem mediaItem)
        {
            MediaItemDTO mediaItemDTO = new MediaItemDTO
            {
                Id = mediaItem.Id,
                Title = mediaItem.Title,
                Type = mediaItem.Type.ToString(),
                Status = mediaItem.Status.ToString(),
                Notes = mediaItem.Notes                
            };

            return mediaItemDTO;
        }

        public static MediaItem DtoToModel(this MediaItemDTO dto)
        {
            MediaItem mediaItem = new MediaItem
            {
                Id = dto.Id,
                Title = dto.Title,
                Type = ParseMediaType(dto.Type),
                Status = ParseStatusType(dto.Status),
                Notes = dto.Notes
            };

            return mediaItem;
        }

        private static MediaType ParseMediaType(string typeString)
        {
            if (string.IsNullOrWhiteSpace(typeString))
            {
                throw new InvalidMediaItemException("MediaType cannot be empty");
            }

            if (Enum.TryParse<MediaType>(typeString, ignoreCase: true, out var result))
            {
                return result;
            }

            throw new InvalidMediaItemException($"'{typeString}' is not a valid MediaType.");
        }

        private static MediaStatus ParseStatusType(string statusString)
        {
            if (string.IsNullOrWhiteSpace(statusString))
            {
                throw new InvalidMediaItemException("StatusType cannot be empty");
            }

            if (Enum.TryParse<MediaStatus>(statusString, ignoreCase: true, out var result))
            {
                return result;
            }

            throw new InvalidMediaItemException($"'{statusString}' is not a valid StatusType.");
        }
    }
}