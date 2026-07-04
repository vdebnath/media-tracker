using Microsoft.AspNetCore.Mvc;
using MediaTracker.Services;
using MediaTracker.Data.Models;

namespace MediaTracker.Api
{
    /// <summary>
    /// Main controller for CRUD operations on media items 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MediaItemsController : ControllerBase
    {
        private readonly IMediaItemService _mediaItemService;

        /// <summary>
        /// Constructor for controller with service injected 
        /// </summary>
        /// <param name="service">Service interface being injected</param>
        public MediaItemsController(IMediaItemService service)
        {
            _mediaItemService = service;
        }

        /// <summary>
        /// Retrieves all media items
        /// </summary>
        /// <returns>List of all media items</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var mediaItems = await _mediaItemService.GetAllAsync();
            return Ok(mediaItems);
        }

        /// <summary>
        /// Retrieves a single media item
        /// </summary>
        /// <param name="id">ID of media item to retrieve</param>
        /// <returns>Single media item matching input ID</returns>
        /// <remarks>404 error if no media item corresponds to input ID</remarks>
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

        /// <summary>
        /// Add a single media item to DB
        /// </summary>
        /// <param name="mediaItem">New media item to add</param>
        /// <returns>201 created with media item that got added</returns>
        [HttpPost]
        public async Task<IActionResult> AddMediaItem(MediaItem mediaItem)
        {
            var addedMediaItem = await _mediaItemService.AddItemAsync(mediaItem);
            return CreatedAtAction(nameof(GetMediaItem), new {id = addedMediaItem.Id}, addedMediaItem);
        }

        /// <summary>
        /// Edits a single media item 
        /// </summary>
        /// <param name="id">ID of media item to edit</param>
        /// <param name="mediaItem">The details of the media item to be eddited</param>
        /// <returns>200 ok with updated media item</returns>
        /// <remarks>Return 401 if the input id does not match the media item</remarks>
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

        /// <summary>
        /// Removes a single media item from DB
        /// </summary>
        /// <param name="id">ID of media item to be removed</param>
        /// <returns>204 No Content if successfull</returns>
        /// <remarks>Returns 404 Not Found if input ID does not match any existing records</remarks>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMediaItem(int id)
        {
            bool gotDeleted = await _mediaItemService.DeleteItemAsync(id);

            if (!gotDeleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}