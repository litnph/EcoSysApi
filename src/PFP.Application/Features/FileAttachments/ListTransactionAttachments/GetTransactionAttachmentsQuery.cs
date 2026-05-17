using MediatR;
using PFP.Application.Features.FileAttachments.Common;

namespace PFP.Application.Features.FileAttachments.ListTransactionAttachments;

public sealed record GetTransactionAttachmentsQuery(Guid TransactionId) : IRequest<GetTransactionAttachmentsResponse>;
