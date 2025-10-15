namespace WebApp3.DTOs;

public record CreateTodoRequest
{
    public required string Title { get; init; }
    public bool IsDone { get; init; }
    public DateTime? DueDate { get; init; }
}

public record UpdateTodoRequest
{
    public required string Title { get; init; }
    public bool IsDone { get; init; }
    public DateTime? DueDate { get; init; }
}

public record TodoResponse
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public bool IsDone { get; init; }
    public DateTime? DueDate { get; init; }
}

public record PaginatedTodosResponse
{
    public required List<TodoResponse> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
