using Microsoft.EntityFrameworkCore;
using MediaTracker.Data.Models;

namespace MediaTracker.Data
{
    /// <summary>
    /// DB Context file for project
    /// </summary>
    public class MediaTrackerDbContext : DbContext
    {
        public DbSet<MediaItem> MediaItems {get; set;}

        /// <summary>
        /// Constructor for DbContext class
        /// </summary>
        /// <param name="mediaTrackerOptions">EF Core options</param>
        public MediaTrackerDbContext(DbContextOptions<MediaTrackerDbContext> mediaTrackerOptions) : base(mediaTrackerOptions)
        {
            
        }
    }
}

