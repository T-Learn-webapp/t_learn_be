namespace TLearn.Domain.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "You don't have permission to access this resource.") 
        : base(message)
    {
    }
}