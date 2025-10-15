//Sonnet 4.5

using WebApp3.DTOs;
using WebApp3.Models;
using WebApp3.Services;
using WebApp3.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<ITodoService, InMemoryTodoService>();

// Add Swagger/OpenAPI services (Development only will be configured below)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add ProblemDetails support
builder.Services.AddProblemDetails();

var app = builder.Build();

// Get AppName from configuration and log it at startup
var appName = app.Configuration["AppName"];
app.Logger.LogInformation("Application starting: {AppName}", appName);

// Enable ProblemDetails middleware
app.UseStatusCodePages();

// Enable Swagger in Development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use structured logging for HTTP requests
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("HTTP {Method} {Path} received", context.Request.Method, context.Request.Path);
    await next();
});

// Health endpoint
app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "OK" });
})
.WithName("GetHealth")
.WithOpenApi();

// Time endpoint
app.MapGet("/time", () =>
{
    return Results.Ok(DateTime.UtcNow.ToString("o"));
})
.WithName("GetTime")
.WithOpenApi();

app.MapGet("/", () => "Hello World!");

// Todo endpoints

// GET /todos - Get all todos with pagination
app.MapGet("/todos", async (ITodoService todoService, int page = 1, int pageSize = 10) =>
{
    if (page < 1)
        return Results.Problem("Page must be greater than 0.", statusCode: 400);
    
    if (pageSize < 1 || pageSize > 100)
        return Results.Problem("PageSize must be between 1 and 100.", statusCode: 400);

    var (items, totalCount) = await todoService.GetAllAsync(page, pageSize);
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    var response = new PaginatedTodosResponse
    {
        Items = items.Select(t => new TodoResponse
        {
            Id = t.Id,
            Title = t.Title,
            IsDone = t.IsDone,
            DueDate = t.DueDate
        }).ToList(),
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount,
        TotalPages = totalPages
    };

    return Results.Ok(response);
})
.WithName("GetTodos")
.WithOpenApi();

// GET /todos/{id} - Get todo by id
app.MapGet("/todos/{id:int}", async (ITodoService todoService, int id) =>
{
    var todo = await todoService.GetByIdAsync(id);
    
    if (todo == null)
        return Results.Problem($"Todo with id {id} not found.", statusCode: 404);

    var response = new TodoResponse
    {
        Id = todo.Id,
        Title = todo.Title,
        IsDone = todo.IsDone,
        DueDate = todo.DueDate
    };

    return Results.Ok(response);
})
.WithName("GetTodoById")
.WithOpenApi();

// POST /todos - Create new todo
app.MapPost("/todos", async (ITodoService todoService, CreateTodoRequest request, HttpContext httpContext) =>
{
    var errors = TodoValidator.Validate(request.Title, request.DueDate);
    
    if (errors.Any())
        return Results.ValidationProblem(errors.ToDictionary(e => e, e => new[] { e }));

    var todo = new Todo
    {
        Title = request.Title,
        IsDone = request.IsDone,
        DueDate = request.DueDate
    };

    var created = await todoService.CreateAsync(todo);

    var response = new TodoResponse
    {
        Id = created.Id,
        Title = created.Title,
        IsDone = created.IsDone,
        DueDate = created.DueDate
    };

    return Results.Created($"/todos/{created.Id}", response);
})
.WithName("CreateTodo")
.WithOpenApi();

// PUT /todos/{id} - Update todo
app.MapPut("/todos/{id:int}", async (ITodoService todoService, int id, UpdateTodoRequest request) =>
{
    var errors = TodoValidator.Validate(request.Title, request.DueDate);
    
    if (errors.Any())
        return Results.ValidationProblem(errors.ToDictionary(e => e, e => new[] { e }));

    var todo = new Todo
    {
        Title = request.Title,
        IsDone = request.IsDone,
        DueDate = request.DueDate
    };

    var updated = await todoService.UpdateAsync(id, todo);
    
    if (updated == null)
        return Results.Problem($"Todo with id {id} not found.", statusCode: 404);

    var response = new TodoResponse
    {
        Id = updated.Id,
        Title = updated.Title,
        IsDone = updated.IsDone,
        DueDate = updated.DueDate
    };

    return Results.Ok(response);
})
.WithName("UpdateTodo")
.WithOpenApi();

// DELETE /todos/{id} - Delete todo
app.MapDelete("/todos/{id:int}", async (ITodoService todoService, int id) =>
{
    var deleted = await todoService.DeleteAsync(id);
    
    if (!deleted)
        return Results.Problem($"Todo with id {id} not found.", statusCode: 404);

    return Results.NoContent();
})
.WithName("DeleteTodo")
.WithOpenApi();

app.Run();
