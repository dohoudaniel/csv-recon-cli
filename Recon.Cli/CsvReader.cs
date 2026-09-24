using System.Globalization;

namespace Recon.Cli;

public static class CsvReader
{
    public static async Task<IReadOnlyList<Transaction>> ReadAsync(
        string path,
        CancellationToken ct = default)
    {
        var rows = new List<Transaction>();
        var lines = await File.ReadAllLinesAsync(path, ct);

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 4)
                throw new FormatException($"Malformed row: {line}");

            rows.Add(new Transaction(
                Reference: parts[0].Trim(),
                Date: DateOnly.ParseExact(parts[1].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Amount: decimal.Parse(parts[2].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture),
                Description: parts[3].Trim()));
        }

        return rows;
    }
}