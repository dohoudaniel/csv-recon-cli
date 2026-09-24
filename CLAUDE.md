# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

A C# (.NET 10) CLI that reconciles a bank statement CSV against a ledger CSV and reports exceptions. Sample inputs live at the repo root: `statement.csv` and `ledger.csv`.

## Commands

The solution file uses the new XML format (`Recon.slnx`), which needs a recent .NET SDK (10.x).

```sh
dotnet build Recon.slnx
dotnet test Recon.slnx
dotnet test --filter "FullyQualifiedName~Recon.Tests.SomeClass.SomeTest"   # single test
dotnet run --project Recon.Cli -- <args>
```

There is no linter or formatter configured beyond the compiler. Nullable reference types are enabled in both projects.

## Architecture

- `Recon.Cli` (exe, namespace `Recon.Cli`), in three layers:
  - `CsvReader.ReadAsync`: parses a CSV into `Transaction` records. It expects the header `Reference,Date,Amount,Description`, dates as `yyyy-MM-dd`, and amounts as invariant-culture `decimal`. It splits naively on `,`, so quoted fields and commas inside fields are not supported. It skips blank lines and throws `FormatException` on rows with fewer than 4 columns.
  - `Reconciler.Reconcile(statement, ledger)`: matches rows by `Reference` (ordinal, so case-sensitive) and returns an `ExceptionKind` per reference. Results are ordered by first appearance, statement references first and then ledger-only ones. A reference that appears more than once on a side is never matched: it gets one `Duplicate*` result per duplicated side (so up to two results for that reference), and both amounts are that side's sums.
  - `Program.cs`: top-level statements. Usage is `<statement.csv> <ledger.csv>`. It prints a fixed-width table and a summary line. Exit codes are 0 (all matched), 1 (exceptions found) and 2 (usage, I/O or format error). The README documents this output and the exit codes, so update it when either changes.
- `Models.cs`: immutable records. `ReconResult.Difference` is computed as `StatementAmount - LedgerAmount` and is null when either side is missing.
- `Recon.Tests`: an xUnit project that references `Recon.Cli`. `Xunit` is a global using. `CsvReaderTests` writes to real temp files.

Money is always `decimal`. Keep parsing culture-invariant.
