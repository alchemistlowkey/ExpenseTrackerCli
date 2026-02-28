# Expense Tracker CLI

A command-line application built with C# for tracking personal expenses. You can add, update, delete, list, summarize, budget, and export your expenses — all from the terminal.

---

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later

---

## Installation

```bash
git clone https://github.com/alchemistlowkey/ExpenseTrackerCli.git
cd ExpenseTrackerCli/ExpenseTrackerCli
dotnet build
```

---

## Usage

All commands follow this pattern:

```bash
dotnet run <command> [arguments]
```

---

### Add an Expense

Add a new expense with today's date:

```bash
dotnet run add "Groceries" 45.50 "food"
```

Add a new expense with a custom date:

```bash
dotnet run add "Groceries" 45.50 "food" --date "2026-07-26"
```

**Valid categories:** `transport`, `food`, `entertainment`, `bills`, `other`

---

### Update an Expense

Update an existing expense by its Id:

```bash
dotnet run update 1 "Grocery Shopping" 50.00 "food"
```

---

### Delete an Expense

Delete an expense by its Id:

```bash
dotnet run delete 1
```

---

### List Expenses

List all expenses:

```bash
dotnet run list
```

List expenses by category:

```bash
dotnet run list --category "food"
```

**Example output:**

```
Id  | Description                    | Amount     | Category        | Date
--------------------------------------------------------------------------------------------------------------
1   | Groceries                      | 45.50      | food            | 2026-07-26
2   | BRT Ticket                     | 1.20       | transport       | 2026-05-26
```

---

### Summary

View total expenses across all time:

```bash
dotnet run summary
```

View total expenses for a specific month and year:

```bash
dotnet run summary --month 07 --year 2026
```

**Example output:**

```
Total expenses for July 2026: $45.50
```

> **Note:** The month must be zero-padded, e.g. `05` for May, `07` for July.

---

### Set a Monthly Budget

Set a monthly spending limit. This is saved in a `config.json` file:

```bash
dotnet run set-budget 500
```

When adding a new expense, you will automatically be notified of your remaining budget or warned if you have exceeded it:

```
⚠️  WARNING: You have exceeded your monthly budget!
   Budget:  $500
   Spent:   $530
   Over by: $30
```

---

### Export to CSV

Export all expenses to a `expenses.csv` file in the project directory:

```bash
dotnet run export
```

**Example output:**

```
Data successfully exported to expenses.csv
```

The CSV file includes the following columns: `Id`, `Description`, `Amount`, `Category`, `Date`.

---

## Data Storage

| File            | Purpose                           |
| --------------- | --------------------------------- |
| `expenses.json` | Stores all expense records        |
| `config.json`   | Stores the monthly budget setting |
| `expenses.csv`  | Generated on export               |

Both JSON files are auto-created in the project directory on first use.

---

## Project Structure

```
ExpenseTrackerCli/
├── ExpenseTrackerCli/
│   ├── Program.cs       # CLI entry point and command handling
│   ├── ExpenseItem.cs      # Expense model
│   ├── expenses.json        # Auto-generated expense data file
|   ├── expenses.csv
|   ├── BudgetConfig.cs
|   └── config.json
```

---

## Date Format

All dates must follow the `yyyy-MM-dd` format, e.g. `2026-07-26`.

---

## Inspiration

This project is based on the [Expense Tracker](https://roadmap.sh/projects/expense-tracker) project idea from [roadmap.sh](https://roadmap.sh).
