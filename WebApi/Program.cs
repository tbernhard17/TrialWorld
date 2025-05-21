using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using TrialWorld.Application;
using TrialWorld.Infrastructure;
using TrialWorld.Infrastructure.FFmpeg;
using TrialWorld.Infrastructure.Search;
using TrialWorld.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TrialWorld API",
        Version = "v1",
        Description = "API for the TrialWorld platform, providing media analysis and search capabilities",
        Contact = new OpenApiContact
        {
            Name = "TrialWorld Support",
            Email = "support@TrialWorld.com"
        }
    });

    // Include XML comments for Swagger documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Register services from other layers
builder.Services.AddApplicationServices();

// Configure and register infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://TrialWorld.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrialWorld API v1"));
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Add global error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");

// Add authentication/authorization middleware here if needed
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// Create necessary directories if they don't exist
var searchOptions = builder.Configuration.GetSection("Search").Get<SearchOptions>();
if (searchOptions != null && !string.IsNullOrEmpty(searchOptions.IndexDirectory))
{
    Directory.CreateDirectory(searchOptions.IndexDirectory);
}

app.Run();
