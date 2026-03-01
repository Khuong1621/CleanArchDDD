namespace Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with key '{key}' was not found.") { }
}

public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string rule) : base(rule) { }
}
