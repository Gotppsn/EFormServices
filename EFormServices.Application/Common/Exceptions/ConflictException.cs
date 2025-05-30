// EFormServices.Application/Common/Exceptions/ConflictException.cs
// Got code 30/05/2025
namespace EFormServices.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string name, object key) 
        : base($"Entity \"{name}\" ({key}) already exists.")
    {
    }
}