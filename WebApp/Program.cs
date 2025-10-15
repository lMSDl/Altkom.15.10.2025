//GPT-4.1

using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Read AppName from configuration
string appName = builder.Configuration["AppName"] ?? "Unknown";

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add Swagger only in Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Minimal API", Version = "v1" });
    });
}

var app = builder.Build();

// Log AppName at startup
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
logger.LogInformation("AppName: {AppName}", appName);

// Use structured logging for requests
app.Use(async (context, next) =>
{
    logger.LogInformation("Request {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Json(new { status = "OK" }));

app.MapGet("/time", () => Results.Json(new { utc = DateTime.UtcNow.ToString("O") }));

// --- Todo resource ---

var todos = new List<TodoApi.Todo>();
var nextId = 1;

// GET /todos (paginated)
app.MapGet("/todos", (int page = 1, int pageSize = 10) =>
{
    if (page < 1 || pageSize < 1 || pageSize > 100)
        return Results.Problem("Invalid pagination parameters.", statusCode: 400);
    var items = todos.Skip((page - 1) * pageSize).Take(pageSize).Select(TodoApi.ToResponse).ToList();
    return Results.Ok(items);
});

// GET /todos/{id}
app.MapGet("/todos/{id}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    return todo is null
        ? Results.Problem("Todo not found.", statusCode: 404)
        : Results.Ok(TodoApi.ToResponse(todo));
});

// POST /todos
app.MapPost("/todos", (TodoApi.TodoRequest req, HttpContext ctx) =>
{
    var validation = TodoApi.ValidateTodo(req);
    if (validation is not null)
        return Results.Problem(validation, statusCode: 400);
    var todo = new TodoApi.Todo(nextId++, req.Title, req.IsDone, req.DueDate);
    todos.Add(todo);
    var url = $"/todos/{todo.Id}";
    return Results.Created(url, TodoApi.ToResponse(todo));
});

// PUT /todos/{id}
app.MapPut("/todos/{id}", (int id, TodoApi.TodoRequest req) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null)
        return Results.Problem("Todo not found.", statusCode: 404);
    var validation = TodoApi.ValidateTodo(req);
    if (validation is not null)
        return Results.Problem(validation, statusCode: 400);
    var updated = new TodoApi.Todo(id, req.Title, req.IsDone, req.DueDate);
    todos[todos.IndexOf(todo)] = updated;
    return Results.Ok(TodoApi.ToResponse(updated));
});

// DELETE /todos/{id}
app.MapDelete("/todos/{id}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null)
        return Results.Problem("Todo not found.", statusCode: 404);
    todos.Remove(todo);
    return Results.NoContent();
});

app.Run();

// --- Todo resource types and helpers ---
class TodoApi
{
    public record Todo(int Id, string Title, bool IsDone, DateTime? DueDate);
    public record TodoRequest(string Title, bool IsDone, DateTime? DueDate);
    public record TodoResponse(int Id, string Title, bool IsDone, DateTime? DueDate);

    public static TodoResponse ToResponse(Todo t) => new(t.Id, t.Title, t.IsDone, t.DueDate);

    public static string? ValidateTodo(TodoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title) || req.Title.Length < 1 || req.Title.Length > 100)
            return "Title is required (1-100 chars).";
        if (req.DueDate is not null && req.DueDate <= DateTime.UtcNow)
            return "DueDate must be in the future.";
        return null;
    }
}
