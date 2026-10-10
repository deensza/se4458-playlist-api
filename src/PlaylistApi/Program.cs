using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using PlaylistApi.Data;
using PlaylistApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Render (and most PaaS hosts) tell the app which port to listen on via the PORT env variable.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers()
    // Enums (e.g. sortBy=Title) are shown and accepted as names instead of numbers.
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// In-memory data store: Singleton, so the same list lives for the whole lifetime of the app.
builder.Services.AddSingleton<ISongRepository, InMemorySongRepository>();

// Business logic layer: Scoped = a new SongService for every HTTP request (it keeps no state itself).
builder.Services.AddScoped<ISongService, SongService>();

// Swagger / OpenAPI (SE4458 - API Design, "Use a swagger")
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Playlist API",
        Version = "v1",
        Description = "SE4458 Assignment 2 – Group 2 (Playlist). Simple REST API with an in-memory data store. " +
                      "A small demo UI that uses this API is available at [/app/](/app/).",
        Contact = new OpenApiContact { Name = "Bermal Deniz Akkaya" }
    });

    // Show the /// <summary>, <remarks>, <response> and <example> comments in Swagger.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile), includeControllerXmlComments: true);
});

// Unexpected errors are returned as a standard ProblemDetails JSON (500) instead of an HTML page.
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Swagger is enabled in every environment because the assignment requires
// a publicly available swagger page on the deployed app.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Playlist API v1");
    options.DocumentTitle = "Playlist API – Swagger";
});

// Optional demo UI (not required by the assignment): wwwroot/app/index.html is served at /app/.
// It only calls the public API endpoints above; the API itself does not depend on it.
app.UseDefaultFiles();
app.UseStaticFiles();

// Opening the root URL takes the visitor straight to the swagger page.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();

app.Run();
