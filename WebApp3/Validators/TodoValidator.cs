using System.ComponentModel.DataAnnotations;

namespace WebApp3.Validators;

public static class TodoValidator
{
    public static ValidationResult? ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new ValidationResult("Title is required.");
        
        if (title.Length < 1 || title.Length > 100)
            return new ValidationResult("Title must be between 1 and 100 characters.");
        
        return ValidationResult.Success;
    }

    public static ValidationResult? ValidateDueDate(DateTime? dueDate)
    {
        if (dueDate.HasValue && dueDate.Value.ToUniversalTime() <= DateTime.UtcNow)
            return new ValidationResult("DueDate must be in the future.");
        
        return ValidationResult.Success;
    }

    public static List<string> Validate(string title, DateTime? dueDate)
    {
        var errors = new List<string>();

        var titleResult = ValidateTitle(title);
        if (titleResult != ValidationResult.Success)
            errors.Add(titleResult!.ErrorMessage!);

        var dueDateResult = ValidateDueDate(dueDate);
        if (dueDateResult != ValidationResult.Success)
            errors.Add(dueDateResult!.ErrorMessage!);

        return errors;
    }
}
