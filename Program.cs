using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings for better API readability
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Use camelCase for JSON properties
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure EF Core with InMemory database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TaskTrackerDb"));

// Configure CORS for local SPA
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalSPA", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure API behavior options
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Customize validation error responses to use ProblemDetails
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)   
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.HttpContext.Request.Path
        };

        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Task Tracker API",
        Version = "v1",
        Description = "A RESTful API for managing tasks with filtering and sorting capabilities"
    });
});

// Add HTTP logging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.Duration;
});

var app = builder.Build();

// Seed the InMemory database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // EnsureCreated will create the InMemory database and apply seed data
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Tracker API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at app root
    });
}

// Global exception handler - RFC 7807 ProblemDetails
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";

        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = "An error occurred while processing your request.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path,
            Detail = app.Environment.IsDevelopment() ? exception?.Message : "An internal server error occurred."
        };

        if (app.Environment.IsDevelopment() && exception != null)
        {
            problemDetails.Extensions["exception"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        context.Response.StatusCode = problemDetails.Status.Value;
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// Enable HTTP logging
app.UseHttpLogging();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowLocalSPA");

app.UseAuthorization();

app.MapControllers();

app.Run();
