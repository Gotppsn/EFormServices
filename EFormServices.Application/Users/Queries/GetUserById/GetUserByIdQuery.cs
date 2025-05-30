// EFormServices.Application/Users/Queries/GetUserById/GetUserByIdQuery.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Models;
using MediatR;

namespace EFormServices.Application.Users.Queries.GetUserById;

public record GetUserByIdQuery(int Id) : IRequest<Result<UserDto>>;