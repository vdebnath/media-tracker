using Microsoft.EntityFrameworkCore;
using MediaTracker.Data.Models;

namespace MediaTracker.Data
{
    public class MediaTrackerDbContext : DbContext
    {
        public DbSet<MediaItem> MediaItems {get; set;}

        public MediaTrackerDbContext(DbContextOptions<MediaTrackerDbContext> mediaTrackerOptions) : base(mediaTrackerOptions)
        {
            
        }
    }
}

