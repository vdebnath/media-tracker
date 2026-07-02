using Microsoft.EntityFrameworkCore;
using MediaTracker.Data;

//Add services to container
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//Register SQLLite connection
builder.Services.RegisterDataDependencies(builder.Configuration, builder.Environment);

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