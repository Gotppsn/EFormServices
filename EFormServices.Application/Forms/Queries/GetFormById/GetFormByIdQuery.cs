// EFormServices.Application/Forms/Queries/GetFormById/GetFormByIdQuery.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Models;
using MediatR;

namespace EFormServices.Application.Forms.Queries.GetFormById;

public record GetFormByIdQuery(int Id) : IRequest<Result<FormDto>>;