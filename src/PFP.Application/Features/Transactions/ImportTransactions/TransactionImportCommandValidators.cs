using FluentValidation;
using PFP.Application.Features.Transactions.CreateTransaction;

namespace PFP.Application.Features.Transactions.ImportTransactions;

public sealed class PreviewTransactionImportCommandValidator : AbstractValidator<PreviewTransactionImportCommand>
{
    public PreviewTransactionImportCommandValidator()
    {
        RuleFor(command => command.Items)
            .NotNull()
            .Must(rows => rows.Count is > 0 and <= 100)
            .WithMessage("An import batch must contain between 1 and 100 rows.")
            .Must(rows => rows.All(row => row.ClientRequestId is not null && row.ClientRequestId != Guid.Empty))
            .WithMessage("Every import row requires a non-empty ClientRequestId.")
            .Must(rows => rows.Select(row => row.ClientRequestId).Distinct().Count() == rows.Count)
            .WithMessage("ClientRequestId values must be unique inside an import batch.");
    }
}

public sealed class CommitTransactionImportCommandValidator : AbstractValidator<CommitTransactionImportCommand>
{
    public CommitTransactionImportCommandValidator()
    {
        RuleFor(command => command.Items)
            .NotNull()
            .Must(rows => rows.Count is > 0 and <= 100)
            .WithMessage("An import batch must contain between 1 and 100 rows.")
            .Must(rows => rows.All(row => row.ClientRequestId is not null && row.ClientRequestId != Guid.Empty))
            .WithMessage("Every import row requires a non-empty ClientRequestId.")
            .Must(rows => rows.Select(row => row.ClientRequestId).Distinct().Count() == rows.Count)
            .WithMessage("ClientRequestId values must be unique inside an import batch.");
    }
}
