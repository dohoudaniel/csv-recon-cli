using System.Globalization;
using Recon.Cli;

// Exit codes: 0 = everything matched, 1 = exceptions found, 2 = usage or input error.
if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: Recon.Cli <statement.csv> <ledger.csv>");
    return 2;
}

IReadOnlyList<Transaction> statement, ledger;
try
{
    statement = await CsvReader.ReadAsync(args[0]);
    ledger = await CsvReader.ReadAsync(args[1]);
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 2;
}

var results = Reconciler.Reconcile(statement, ledger);

static string Money(decimal? value) =>
    value?.ToString("N2", CultureInfo.InvariantCulture) ?? "-";

Console.WriteLine($"{"Reference",-12} {"Status",-22} {"Statement",14} {"Ledger",14} {"Difference",14}");
foreach (var r in results)
{
    Console.WriteLine(
        $"{r.Reference,-12} {r.Kind,-22} {Money(r.StatementAmount),14} {Money(r.LedgerAmount),14} {Money(r.Difference),14}");
}

var exceptions = results.Count(r => r.Kind != ExceptionKind.Matched);
Console.WriteLine();
Console.WriteLine($"{results.Count} references, {results.Count - exceptions} matched, {exceptions} exceptions.");

return exceptions == 0 ? 0 : 1;
