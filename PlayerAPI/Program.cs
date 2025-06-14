using PlayerAPI.Models;
using PlayerAPI.Models.Config;
using PlayerAPI.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

var playerConfig = builder.Configuration.GetSection("PlayerConfig").Get<PlayerConfig>();
var gpioConfig = builder.Configuration.GetSection("GPIOConfig").Get<GPIOCofig>() ?? new GPIOCofig();
var volumeConfig = builder.Configuration.GetSection("VolumeConfig").Get<VolumeConfig>() ?? new VolumeConfig();

if (playerConfig == null)
{
    throw new ConfigurationErrorsException("Cannot find \"PlayerConfig\".");
}



builder.Services.AddSingleton<PlayerConfig>(sw => playerConfig);
builder.Services.AddSingleton<GPIOCofig>(sw => gpioConfig);
builder.Services.AddSingleton<VolumeConfig>(sw => volumeConfig);


builder.Services.AddSingleton<IVolumeHandler, VolumeHandler>();

builder.Services.AddSingleton<FileContext>();
builder.Services
    .AddSingleton<IPlaylistProvider, PlaylistProvider>();

builder.Services.AddSingleton<IGPIOController, LedController>();
builder.Services.AddSingleton<IMyPlayer, MPlayer>();
builder.Services.AddSingleton<ButtonController>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();

    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Services.GetService<ButtonController>();
app.Services.GetService<IMyPlayer>();
app.Services.GetService<IVolumeHandler>();
app.Services.GetService<IPlaylistProvider>()?.GetAllPlaylists();
app.Services.GetService<IGPIOController>()?.ShowPower();
app.Run();

