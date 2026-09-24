using Recon.Cli;

namespace Recon.Tests;

public sealed class CsvReaderTests : IDisposable
{
    private readonly string _path = Path.GetTempFileName();

    public void Dispose() => File.Delete(_path);

    [Fact]
    public async Task ReadsRows_SkippingHeaderAndBlankLines()
    {
        await File.WriteAllTextAsync(_path,
            "Reference,Date,Amount,Description\n" +
            "TXN1, 2026-09-20 ,-2500.50, Card payment \n" +
            "\n");

        var row = Assert.Single(await CsvReader.ReadAsync(_path));

        Assert.Equal(new Transaction("TXN1", new DateOnly(2026, 9, 20), -2500.50m, "Card payment"), row);
    }

    [Fact]
    public async Task RowWithTooFewColumns_Throws()
    {
        await File.WriteAllTextAsync(_path, "Reference,Date,Amount,Description\nTXN1,2026-09-20,1.00\n");

        await Assert.ThrowsAsync<FormatException>(() => CsvReader.ReadAsync(_path));
    }

    [Fact]
    public async Task NonIsoDate_Throws()
    {
        await File.WriteAllTextAsync(_path, "Reference,Date,Amount,Description\nTXN1,20/09/2026,1.00,x\n");

        await Assert.ThrowsAsync<FormatException>(() => CsvReader.ReadAsync(_path));
    }
}
