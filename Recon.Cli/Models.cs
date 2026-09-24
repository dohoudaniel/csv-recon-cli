namespace Recon.Cli;

public sealed record Transaction(
    string Reference,
    DateOnly Date,
    decimal Amount,
    string Description);

public enum ExceptionKind
{
    Matched,
    AmountMismatch,
    MissingInLedger,
    MissingInStatement,
    DuplicateInLedger,
    DuplicateInStatement
}

public sealed record ReconResult(
    string Reference,
    ExceptionKind Kind,
    decimal? StatementAmount,
    decimal? LedgerAmount)
{
    public decimal? Difference =>
        StatementAmount is { } s && LedgerAmount is { } l ? s - l : null;
}