namespace TLearn.Domain.Exceptions;

public class SqlException : DomainException
{
    public SqlException(string message, Exception innerException) 
        : base($"Database error: {message}", innerException)
    {
    }
}

public class DuplicateEntryException : SqlException
{
    public DuplicateEntryException(string entityName, string fieldName, string value, Exception innerException) 
        : base($"A {entityName} with {fieldName} '{value}' already exists.", innerException)
    {
    }
}

public class ForeignKeyViolationException : SqlException
{
    public ForeignKeyViolationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}