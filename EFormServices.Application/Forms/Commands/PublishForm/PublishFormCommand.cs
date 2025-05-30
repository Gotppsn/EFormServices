// EFormServices.Application/Forms/Commands/PublishForm/PublishFormCommand.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Models;
using MediatR;

namespace EFormServices.Application.Forms.Commands.PublishForm;

public record PublishFormCommand(int FormId) : IRequest<Result>;