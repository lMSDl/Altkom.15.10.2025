using WebApp3.Models;

namespace WebApp3.Services;

public interface ITodoService
{
    Task<(List<Todo> items, int totalCount)> GetAllAsync(int page, int pageSize);
    Task<Todo?> GetByIdAsync(int id);
    Task<Todo> CreateAsync(Todo todo);
    Task<Todo?> UpdateAsync(int id, Todo todo);
    Task<bool> DeleteAsync(int id);
}

public class InMemoryTodoService : ITodoService
{
    private readonly List<Todo> _todos = new();
    private int _nextId = 1;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<(List<Todo> items, int totalCount)> GetAllAsync(int page, int pageSize)
    {
        await _lock.WaitAsync();
        try
        {
            var totalCount = _todos.Count;
            var items = _todos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            return (items, totalCount);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Todo?> GetByIdAsync(int id)
    {
        await _lock.WaitAsync();
        try
        {
            return _todos.FirstOrDefault(t => t.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Todo> CreateAsync(Todo todo)
    {
        await _lock.WaitAsync();
        try
        {
            todo.Id = _nextId++;
            _todos.Add(todo);
            return todo;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Todo?> UpdateAsync(int id, Todo todo)
    {
        await _lock.WaitAsync();
        try
        {
            var existing = _todos.FirstOrDefault(t => t.Id == id);
            if (existing == null)
                return null;

            existing.Title = todo.Title;
            existing.IsDone = todo.IsDone;
            existing.DueDate = todo.DueDate;
            
            return existing;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _lock.WaitAsync();
        try
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
                return false;

            _todos.Remove(todo);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }
}
