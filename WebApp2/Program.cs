//GPT-5

using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services required for OpenAPI/Swagger generation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appName = builder.Configuration["AppName"] ?? "UnknownApp";

var app = builder.Build();

// Log application startup with structured logging
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
startupLogger.LogInformation("Application {AppName} starting at {StartTimeUtc}", appName, DateTime.UtcNow);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Structured request logging middleware
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();
    var reqLogger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Request");
    reqLogger.LogInformation(
        "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        sw.ElapsedMilliseconds);
});

app.MapGet("/health", () => new { status = "OK" })
   .WithName("Health");

app.MapGet("/time", () => DateTime.UtcNow.ToString("O"))
   .WithName("Time");

app.MapGet("/", () => $"{appName} is running");

// In-memory Todo storage
var todos = new List<Todo>();
int nextId = 0; // will increment before use

TodoDto ToDto(Todo t) => new(t.Id, t.Title, t.IsDone, t.DueDate);

// Validation helper
(IDictionary<string, string[]>? errors, bool isValid) Validate(string title, DateTime? dueDate)
{
    var dict = new Dictionary<string, List<string>>();

    if (string.IsNullOrWhiteSpace(title))
    {
        dict.TryAdd("Title", new());
        dict["Title"].Add("Title is required.");
    }
    else if (title.Trim().Length is < 1 or > 100)
    {
        dict.TryAdd("Title", new());
        dict["Title"].Add("Title length must be 1-100 characters.");
    }

    if (dueDate is not null && dueDate <= DateTime.UtcNow)
    {
        dict.TryAdd("DueDate", new());
        dict["DueDate"].Add("DueDate must be in the future.");
    }

    if (dict.Count == 0) return (null, true);

    return (dict.ToDictionary(k => k.Key, v => v.Value.ToArray()), false);
}

// GET /todos (paginated)
app.MapGet("/todos", (int? page, int? pageSize) =>
{
    var p = page.GetValueOrDefault(1);
    var ps = pageSize.GetValueOrDefault(10);
    if (p < 1) p = 1;
    if (ps < 1) ps = 10;

    List<Todo> snapshot;
    lock (todos)
    {
        snapshot = todos.ToList();
    }
    var total = snapshot.Count;
    var items = snapshot.Skip((p - 1) * ps).Take(ps).Select(ToDto).ToList();
    return Results.Ok(new { page = p, pageSize = ps, total, items });
}).WithName("GetTodos");

// GET /todos/{id}
app.MapGet("/todos/{id:int}", (int id) =>
{
    Todo? found;
    lock (todos)
    {
        found = todos.FirstOrDefault(t => t.Id == id);
    }
    return found is null
        ? Results.Problem(statusCode: 404, title: "Not Found", detail: $"Todo {id} not found")
        : Results.Ok(ToDto(found));
}).WithName("GetTodoById");

// POST /todos
app.MapPost("/todos", (CreateTodoRequest req) =>
{
    var (errors, valid) = Validate(req.Title, req.DueDate);
    if (!valid)
    {
        return Results.ValidationProblem(errors!);
    }

    var id = Interlocked.Increment(ref nextId);
    var todo = new Todo(id, req.Title.Trim(), req.IsDone, req.DueDate);
    lock (todos) { todos.Add(todo); }
    return Results.Created($"/todos/{todo.Id}", ToDto(todo));
}).WithName("CreateTodo");

// PUT /todos/{id}
app.MapPut("/todos/{id:int}", (int id, UpdateTodoRequest req) =>
{
    var (errors, valid) = Validate(req.Title, req.DueDate);
    if (!valid)
    {
        return Results.ValidationProblem(errors!);
    }

    lock (todos)
    {
        var idx = todos.FindIndex(t => t.Id == id);
        if (idx == -1)
        {
            return Results.Problem(statusCode: 404, title: "Not Found", detail: $"Todo {id} not found");
        }
        var existing = todos[idx];
        todos[idx] = existing with { Title = req.Title.Trim(), IsDone = req.IsDone, DueDate = req.DueDate };
    }
    return Results.NoContent();
}).WithName("UpdateTodo");

// DELETE /todos/{id}
app.MapDelete("/todos/{id:int}", (int id) =>
{
    lock (todos)
    {
        var idx = todos.FindIndex(t => t.Id == id);
        if (idx == -1)
        {
            return Results.Problem(statusCode: 404, title: "Not Found", detail: $"Todo {id} not found");
        }
        todos.RemoveAt(idx);
    }
    return Results.NoContent();
}).WithName("DeleteTodo");

app.Run();

// Record types (must appear after all top-level statements or before them, not in-between)
record Todo(int Id, string Title, bool IsDone, DateTime? DueDate);
record TodoDto(int Id, string Title, bool IsDone, DateTime? DueDate);
record CreateTodoRequest(string Title, bool IsDone, DateTime? DueDate);
record UpdateTodoRequest(string Title, bool IsDone, DateTime? DueDate);
