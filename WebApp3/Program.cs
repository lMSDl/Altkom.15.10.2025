//Sonnet 4.5

using WebApp3.DTOs;
using WebApp3.Models;
using WebApp3.Services;
using WebApp3.Validators;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<ITodoService, InMemoryTodoService>();

// Add Swagger/OpenAPI services (Development only will be configured below)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add ProblemDetails support
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

// Get AppName from configuration and log it at startup
string? appName = app.Configuration["AppName"];
app.Logger.LogInformation("Application starting: {AppName}", appName);

// Enable ProblemDetails middleware
app.UseStatusCodePages();

// Enable Swagger in Development only
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

// Use structured logging for HTTP requests
app.Use(async (context, next) =>
{
    ILogger<Program> logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
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
    {
        return Results.Problem("Page must be greater than 0.", statusCode: 400);
    }

    if (pageSize is < 1 or > 100)
    {
        return Results.Problem("PageSize must be between 1 and 100.", statusCode: 400);
    }

    (List<Todo> items, int totalCount) = await todoService.GetAllAsync(page, pageSize);
    int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    PaginatedTodosResponse response = new()
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
    Todo? todo = await todoService.GetByIdAsync(id);

    if (todo == null)
    {
        return Results.Problem($"Todo with id {id} not found.", statusCode: 404);
    }

    TodoResponse response = new()
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
    List<string> errors = TodoValidator.Validate(request.Title, request.DueDate);

    if (errors.Any())
    {
        return Results.ValidationProblem(errors.ToDictionary(e => e, e => new[] { e }));
    }

    Todo todo = new()
    {
        Title = request.Title,
        IsDone = request.IsDone,
        DueDate = request.DueDate
    };

    Todo created = await todoService.CreateAsync(todo);

    TodoResponse response = new()
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
    List<string> errors = TodoValidator.Validate(request.Title, request.DueDate);

    if (errors.Any())
    {
        return Results.ValidationProblem(errors.ToDictionary(e => e, e => new[] { e }));
    }

    Todo todo = new()
    {
        Title = request.Title,
        IsDone = request.IsDone,
        DueDate = request.DueDate
    };

    Todo? updated = await todoService.UpdateAsync(id, todo);

    if (updated == null)
    {
        return Results.Problem($"Todo with id {id} not found.", statusCode: 404);
    }

    TodoResponse response = new()
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
    bool deleted = await todoService.DeleteAsync(id);

    return !deleted ? Results.Problem($"Todo with id {id} not found.", statusCode: 404) : Results.NoContent();
})
.WithName("DeleteTodo")
.WithOpenApi();

app.Run();
