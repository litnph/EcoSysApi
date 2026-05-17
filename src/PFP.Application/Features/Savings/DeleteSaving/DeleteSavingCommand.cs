using MediatR;

namespace PFP.Application.Features.Savings.DeleteSaving;

public sealed record DeleteSavingCommand(Guid Id) : IRequest<DeleteSavingResponse>;
