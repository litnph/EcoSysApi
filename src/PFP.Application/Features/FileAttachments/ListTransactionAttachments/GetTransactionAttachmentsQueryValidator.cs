using FluentValidation;

namespace PFP.Application.Features.FileAttachments.ListTransactionAttachments;

public sealed class GetTransactionAttachmentsQueryValidator : AbstractValidator<GetTransactionAttachmentsQuery>
{
    public GetTransactionAttachmentsQueryValidator() => RuleFor(x => x.TransactionId).NotEmpty();
}
