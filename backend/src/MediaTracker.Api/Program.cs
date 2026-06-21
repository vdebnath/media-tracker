using Microsoft.EntityFrameworkCore;
using MediaTracker.Data;

//Add services to container
var builder = WebApplication.CreateBuilder(args);

//Register controller support
builder.Services.AddControllers();

var dbDirectoryConfig = builder.Configuration["Database:Directory"]
    ?? throw new InvalidOperationException("Missing configuration: Database:Directory"); 

var dbFileNameConfig = builder.Configuration["Database:FileName"]
    ?? throw new InvalidOperationException("Missing configuration: Database:FileName");

var dbDirectory = Path.Combine(
    builder.Environment.ContentRootPath,
    dbDirectoryConfig
);

Directory.CreateDirectory(dbDirectory);

var dbPath = Path.Combine(dbDirectory, dbFileNameConfig);

//Add connection string, gets from appSettings
var connectionString = $"Data Source={dbPath}";

//EF Cors & DbContext registration
builder.Services.AddDbContext<MediaTrackerDbContext>(mediaTrackerOptions =>
    mediaTrackerOptions.UseSqlite(connectionString));

//OpenAPI 
builder.Services.AddOpenApi();

var app = builder.Build();

//Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();