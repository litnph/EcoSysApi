using MediatR;
using PFP.Application.Features.Transactions.Common;

namespace PFP.Application.Features.Transactions.GetTransactionById;

/// <summary>Loads one transaction with source and category details.</summary>
public sealed record GetTransactionByIdQuery(Guid Id) : IRequest<GetTransactionByIdResponse>;
