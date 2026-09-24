using Recon.Cli;

namespace Recon.Tests;

public class ReconcilerTests
{
    private static Transaction Txn(string reference, decimal amount) =>
        new(reference, new DateOnly(2026, 9, 20), amount, "test");

    [Fact]
    public void EqualAmounts_AreMatched()
    {
        var result = Assert.Single(Reconciler.Reconcile([Txn("A", 10m)], [Txn("A", 10m)]));

        Assert.Equal(ExceptionKind.Matched, result.Kind);
        Assert.Equal(0m, result.Difference);
    }

    [Fact]
    public void DifferentAmounts_AreMismatch_WithStatementMinusLedgerDifference()
    {
        var result = Assert.Single(Reconciler.Reconcile([Txn("A", -2500.50m)], [Txn("A", -2500.00m)]));

        Assert.Equal(ExceptionKind.AmountMismatch, result.Kind);
        Assert.Equal(-0.50m, result.Difference);
    }

    [Fact]
    public void StatementOnly_IsMissingInLedger()
    {
        var result = Assert.Single(Reconciler.Reconcile([Txn("A", 5m)], []));

        Assert.Equal(ExceptionKind.MissingInLedger, result.Kind);
        Assert.Equal(5m, result.StatementAmount);
        Assert.Null(result.LedgerAmount);
        Assert.Null(result.Difference);
    }

    [Fact]
    public void LedgerOnly_IsMissingInStatement()
    {
        var result = Assert.Single(Reconciler.Reconcile([], [Txn("A", 5m)]));

        Assert.Equal(ExceptionKind.MissingInStatement, result.Kind);
        Assert.Null(result.StatementAmount);
    }

    [Fact]
    public void DuplicateInLedger_IsReported_WithSummedAmounts()
    {
        var result = Assert.Single(Reconciler.Reconcile([Txn("A", 100m)], [Txn("A", 100m), Txn("A", 100m)]));

        Assert.Equal(ExceptionKind.DuplicateInLedger, result.Kind);
        Assert.Equal(100m, result.StatementAmount);
        Assert.Equal(200m, result.LedgerAmount);
    }

    [Fact]
    public void DuplicatesOnBothSides_ProduceOneResultPerSide()
    {
        var results = Reconciler.Reconcile([Txn("A", 1m), Txn("A", 1m)], [Txn("A", 1m), Txn("A", 1m)]);

        Assert.Equal(
            [ExceptionKind.DuplicateInStatement, ExceptionKind.DuplicateInLedger],
            results.Select(r => r.Kind));
    }

    [Fact]
    public void Results_AreOrdered_StatementFirstThenLedgerOnly()
    {
        var results = Reconciler.Reconcile(
            [Txn("B", 1m), Txn("A", 1m)],
            [Txn("C", 1m), Txn("A", 1m)]);

        Assert.Equal(["B", "A", "C"], results.Select(r => r.Reference));
    }

    [Fact]
    public void References_AreCaseSensitive()
    {
        var results = Reconciler.Reconcile([Txn("a", 1m)], [Txn("A", 1m)]);

        Assert.Equal(
            [ExceptionKind.MissingInLedger, ExceptionKind.MissingInStatement],
            results.Select(r => r.Kind));
    }
}
