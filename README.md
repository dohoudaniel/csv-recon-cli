# csv-recon-cli

A C# command-line tool that reconciles a bank statement CSV against a ledger CSV. It matches transactions by reference and reports every exception.

## Requirements

- .NET SDK 10.0 or later

## Usage

```sh
dotnet run --project Recon.Cli -- <statement.csv> <ledger.csv>
```

Try it with the sample files in the repo root:

```sh
dotnet run --project Recon.Cli -- statement.csv ledger.csv
```

```
Reference    Status                      Statement         Ledger     Difference
TXN1001      DuplicateInLedger           15,000.00      30,000.00     -15,000.00
TXN1002      AmountMismatch              -2,500.50      -2,500.00          -0.50
TXN1003      Matched                      7,800.00       7,800.00           0.00
TXN1004      MissingInLedger             -1,200.00              -              -
TXN1006      MissingInLedger              9,000.00              -              -
TXN1005      MissingInStatement                  -        -450.00              -

6 references, 1 matched, 5 exceptions.
```

### Exit codes

| Code | Meaning |
|------|---------|
| 0 | Every reference matched |
| 1 | One or more exceptions were found |
| 2 | Wrong arguments, or an input file is missing or malformed |

## Input format

Both files use the same layout. The first line is a header and is skipped:

```
Reference,Date,Amount,Description
TXN1001,2026-09-20,15000.00,Inbound transfer
```

- **Date** must be `yyyy-MM-dd`.
- **Amount** is a decimal with `.` as the separator. Negative values are debits.
- Blank lines are ignored.
- Fields are split on plain commas. Quoted fields and commas inside a field are **not** supported.

## Statuses

| Status | Meaning |
|--------|---------|
| `Matched` | The reference is on both sides once, with equal amounts |
| `AmountMismatch` | The reference is on both sides once, but the amounts differ |
| `MissingInLedger` | The reference is only on the statement |
| `MissingInStatement` | The reference is only in the ledger |
| `DuplicateInStatement` / `DuplicateInLedger` | The reference appears more than once on that side. It is not matched, and the amounts shown are the totals for each side |

References are matched exactly, including case. **Difference** is the statement amount minus the ledger amount.

## Development

```sh
dotnet build Recon.slnx
dotnet test Recon.slnx
```
