using Microsoft.AspNetCore.Mvc;
using MediaTracker.Services;
using MediaTracker.Data.Models;

namespace MediaTracker.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaItemsController : ControllerBase
    {
        private readonly IMediaItemService _mediaItemService;

        public MediaItemsController(IMediaItemService service)
        {
            _mediaItemService = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var mediaItems = await _mediaItemService.GetAllAsync();
            return Ok(mediaItems);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMediaItem(int id)
        {
            var mediaItem = await _mediaItemService.GetByIdAsync(id);

            if (mediaItem == null)
            {
                return NotFound();
            }

            return Ok(mediaItem);
        }

        [HttpPost]
        public async Task<IActionResult> AddMediaItem(MediaItem mediaItem)
        {
            var addedMediaItem = await _mediaItemService.AddItemAsync(mediaItem);
            return CreatedAtAction(nameof(GetMediaItem), new {id = addedMediaItem.Id}, addedMediaItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMediaItem(int id, MediaItem mediaItem)
        {
            if (id != mediaItem.Id)
            {
                return BadRequest("Route Id does not match media item id.");
            }
            
            var updatedMediaItem = await _mediaItemService.UpdateItemAsync(mediaItem);

            return Ok(updatedMediaItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMediaItem(int id)
        {
            bool gotDeleted = await _mediaItemService.DeleteItemAsync(id);

            if(!gotDeleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}