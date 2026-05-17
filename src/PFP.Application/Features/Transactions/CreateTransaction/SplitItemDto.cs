namespace PFP.Application.Features.Transactions.CreateTransaction;

/// <summary>One participant line when creating a <see cref="PFP.Domain.Enums.TransactionType.Split"/> transaction.</summary>
public sealed record SplitItemDto(string PersonName, string? PersonContact, decimal Amount);
