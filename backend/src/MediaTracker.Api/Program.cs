using MediaTracker.Data;
using MediaTracker.Services;

//Add services to container
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//Register SQLLite connection
builder.Services.RegisterDataDependencies(builder.Configuration, builder.Environment);

builder.Services.AddSwaggerGen();
//builder.Services.AddSwaggerUI();

//Register Services for Business Logic
builder.Services.RegisterServiceDependencies();

//Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

//Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

//Start up cors before mapping controllers
app.UseCors("AllowAngularDev");

app.MapControllers();

app.Run();