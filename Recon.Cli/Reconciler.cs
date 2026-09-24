namespace Recon.Cli;

public static class Reconciler
{
    // Results are ordered by first appearance: statement references first, then ledger-only ones.
    // A reference that appears more than once on a side is not matched; it gets one Duplicate*
    // result per duplicated side, carrying the summed amounts of both sides.
    public static IReadOnlyList<ReconResult> Reconcile(
        IReadOnlyList<Transaction> statement,
        IReadOnlyList<Transaction> ledger)
    {
        var statementByRef = statement.ToLookup(t => t.Reference, StringComparer.Ordinal);
        var ledgerByRef = ledger.ToLookup(t => t.Reference, StringComparer.Ordinal);

        var references = statement.Select(t => t.Reference)
            .Concat(ledger.Select(t => t.Reference))
            .Distinct(StringComparer.Ordinal);

        var results = new List<ReconResult>();

        foreach (var reference in references)
        {
            var s = statementByRef[reference].ToList();
            var l = ledgerByRef[reference].ToList();

            decimal? statementAmount = s.Count > 0 ? s.Sum(t => t.Amount) : null;
            decimal? ledgerAmount = l.Count > 0 ? l.Sum(t => t.Amount) : null;

            if (s.Count > 1 || l.Count > 1)
            {
                if (s.Count > 1)
                    results.Add(new ReconResult(reference, ExceptionKind.DuplicateInStatement, statementAmount, ledgerAmount));
                if (l.Count > 1)
                    results.Add(new ReconResult(reference, ExceptionKind.DuplicateInLedger, statementAmount, ledgerAmount));
                continue;
            }

            var kind = (s.Count, l.Count) switch
            {
                (1, 0) => ExceptionKind.MissingInLedger,
                (0, 1) => ExceptionKind.MissingInStatement,
                _ when statementAmount == ledgerAmount => ExceptionKind.Matched,
                _ => ExceptionKind.AmountMismatch
            };

            results.Add(new ReconResult(reference, kind, statementAmount, ledgerAmount));
        }

        return results;
    }
}
