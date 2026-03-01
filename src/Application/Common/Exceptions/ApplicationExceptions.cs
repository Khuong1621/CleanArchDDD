using FluentValidation.Results;

namespace Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key) 
        : base($"{name} ({key}) was not found.") { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("You do not have permission to perform this action.") { }
}
