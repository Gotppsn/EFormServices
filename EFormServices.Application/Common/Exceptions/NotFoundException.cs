// EFormServices.Application/Common/Exceptions/NotFoundException.cs
// Got code 30/05/2025
namespace EFormServices.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key) 
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}