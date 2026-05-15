namespace TLearn.Domain.Exceptions;

public class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    
    protected DomainException(string message,Exception innerException) : base(message) { }
}