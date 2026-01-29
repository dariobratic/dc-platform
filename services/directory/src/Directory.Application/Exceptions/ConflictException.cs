namespace Directory.Application.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string entityName, string propertyName, object value)
        : base($"{entityName} with {propertyName} '{value}' already exists.")
    {
    }
}
