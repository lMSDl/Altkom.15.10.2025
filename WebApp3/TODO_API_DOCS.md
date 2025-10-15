# Todo API Documentation

## Overview
Complete in-memory CRUD API for Todo resources with validation, pagination, and proper error handling.

## Endpoints

### GET /todos
Returns paginated list of todos.

**Query Parameters:**
- `page` (optional, default: 1) - Page number (must be ? 1)
- `pageSize` (optional, default: 10) - Items per page (1-100)

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": 1,
      "title": "Sample Todo",
      "isDone": false,
      "dueDate": "2025-12-31T23:59:59Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

### GET /todos/{id}
Returns a specific todo by ID.

**Response (200 OK):**
```json
{
  "id": 1,
  "title": "Sample Todo",
  "isDone": false,
  "dueDate": "2025-12-31T23:59:59Z"
}
```

**Response (404 Not Found):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Todo with id 1 not found."
}
```

### POST /todos
Creates a new todo.

**Request Body:**
```json
{
  "title": "New Todo",
  "isDone": false,
  "dueDate": "2025-12-31T23:59:59Z"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "title": "New Todo",
  "isDone": false,
  "dueDate": "2025-12-31T23:59:59Z"
}
```
**Headers:**
- `Location: /todos/1`

**Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title must be between 1 and 100 characters.": ["Title must be between 1 and 100 characters."]
  }
}
```

### PUT /todos/{id}
Updates an existing todo.

**Request Body:**
```json
{
  "title": "Updated Todo",
  "isDone": true,
  "dueDate": null
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "title": "Updated Todo",
  "isDone": true,
  "dueDate": null
}
```

**Response (404 Not Found):**
ProblemDetails with 404 status code.

### DELETE /todos/{id}
Deletes a todo.

**Response (204 No Content):**
Empty body on successful deletion.

**Response (404 Not Found):**
ProblemDetails with 404 status code.

## Validation Rules

### Title
- **Required**: Cannot be null or empty
- **Length**: Must be between 1 and 100 characters

### DueDate
- **Optional**: Can be null
- **Future Date**: If provided, must be in the future (UTC)

## Architecture

### Models (`Models/Todo.cs`)
Domain model representing a Todo item.

### DTOs (`DTOs/TodoDtos.cs`)
- `CreateTodoRequest` - For POST requests
- `UpdateTodoRequest` - For PUT requests
- `TodoResponse` - For responses
- `PaginatedTodosResponse` - For paginated lists

### Services (`Services/TodoService.cs`)
- `ITodoService` - Interface for Todo operations
- `InMemoryTodoService` - Thread-safe in-memory implementation using SemaphoreSlim

### Validators (`Validators/TodoValidator.cs`)
Centralized validation logic for Todo fields.

## Error Handling
- **400 Bad Request**: Validation errors (ProblemDetails with validation details)
- **404 Not Found**: Resource not found (ProblemDetails)
- All errors return RFC 9110 compliant ProblemDetails responses

## Testing with Swagger
Run the application in Development mode and navigate to `/swagger` to test all endpoints interactively.
