// EFormServices.Application/Common/Exceptions/ForbiddenAccessException.cs
// Got code 30/05/2025
namespace EFormServices.Application.Common.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base("Access to this resource is forbidden.")
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }
}