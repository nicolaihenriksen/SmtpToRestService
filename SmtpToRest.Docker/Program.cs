using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SmtpToRest;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<SmtpServerBackgroundService>();

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.Run();