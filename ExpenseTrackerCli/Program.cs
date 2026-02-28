using System.Globalization;
using System.Text.Json;
using ExpenseTrackerCli;

const string FilePath = "expenses.json";

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

// Handle command-line arguments
if (args.Length == 0)
{
    Console.WriteLine("""
        Expense Tracker CLI Usage:
        expense-cli add "expense description" amount "expense category" - Adds a new expense
        expense-cli add "expense description" amount "expense category" --date "expense date" - Adds a new expense
        expense-cli update <id> "New description" amount "expense category" - Updates a expense by Id
        expense-cli delete <id> - Deletes a expense
        expense-cli list - Lists all expenses
        expense-cli list --category "expense category" - Lists expenses by category
        expense-cli summary - Lists a summary of expenses
        expense-cli summary --month "expense month" --year "expense year" - Lists a summary of expenses by month
        expense-cli set-budget amount - Sets a budget
        expense-cli export - Exports expenses to a CSV file
    """);

    return;
}

string command = args[0].ToLower();

if (command == "add")
{

    if (args.Length < 4)
    {
        throw new ArgumentException("Please provide a description, amount, and category for your expense.");
    }

    var description = args[1];
    var amount = decimal.Parse(args[2]);
    var category = args[3];

    if (amount <= 0)
    {
        throw new ArgumentException("Expense amount must be greater than zero.");
    }

    if (category == string.Empty)
    {
        throw new ArgumentException("Expense category cannot be empty.");
    }

    if (category.Trim().ToLower() != "transport" && category.Trim().ToLower() != "food" && category.Trim().ToLower() != "entertainment" && category.Trim().ToLower() != "bills" && category.Trim().ToLower() != "other")
    {
        throw new ArgumentException("Expense category must be 'transport', 'food', 'entertainment', 'bills', or 'other'");
    }


    List<ExpenseItem> expenseItems;

    if (!File.Exists(FilePath) || new FileInfo(FilePath).Length == 0)
    {
        expenseItems = [];
    }
    else
    {
        string json = File.ReadAllText(FilePath);
        expenseItems = JsonSerializer.Deserialize<List<ExpenseItem>>(json, jsonOptions) ?? [];
    }

    var newId = expenseItems.Count > 0 ? expenseItems.Max(t => t.Id) + 1 : 1;

    if (args.Length == 4)
    {
        var expenseToBeAdded = new ExpenseItem
        {
            Id = newId,
            Description = description,
            Amount = amount,
            Category = category,
        };
        expenseItems.Add(expenseToBeAdded);
    }
    else
    {
        if (args[4] == "--date")
        {
            var date = args[5];
            var expenseToBeAdded = new ExpenseItem
            {
                Id = newId,
                Description = description,
                Amount = amount,
                Category = category,
                Date = date
            };
            expenseItems.Add(expenseToBeAdded);
        }
    }

    File.WriteAllText(FilePath, JsonSerializer.Serialize(expenseItems, jsonOptions));

    const string ConfigPath = "config.json";

    if (File.Exists(ConfigPath) && new FileInfo(ConfigPath).Length > 0)
    {
        var config = JsonSerializer.Deserialize<BudgetConfig>(File.ReadAllText(ConfigPath), jsonOptions);

        if (config != null && config.MonthlyBudget > 0)
        {
            // Sum expenses for the current month and year
            var now = DateTime.Now;
            decimal monthlyTotal = 0;

            foreach (var expenseItem in expenseItems)
            {
                if (DateTime.TryParseExact(expenseItem.Date, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime date)
                    && date.Month == now.Month && date.Year == now.Year)
                {
                    monthlyTotal += expenseItem.Amount;
                }
            }

            if (monthlyTotal > config.MonthlyBudget)
            {
                Console.WriteLine($"""

                Expense added successfully
                ⚠️  WARNING: You have exceeded your monthly budget of ${config.MonthlyBudget}!

                   Budget:  ${config.MonthlyBudget}
                   Spent:   ${monthlyTotal}
                   Over by: ${monthlyTotal - config.MonthlyBudget}

                """);
            }
            else
            {
                Console.WriteLine($"Budget remaining for this month: ${config.MonthlyBudget - monthlyTotal}");
            }
        }
    }
}

if (command == "update")
{
    if (args.Length < 5)
    {
        throw new ArgumentException("Please provide an Id, description, amount, and category for your expense.");
    }

    var id = args[1];
    if (!int.TryParse(id, out int expenseId))
    {
        throw new ArgumentException("Invalid Id format");
    }

    var description = args[2];
    var amount = decimal.Parse(args[3]);
    var category = args[4];

    if (amount <= 0)
    {
        throw new ArgumentException("Expense amount must be greater than zero.");
    }

    if (category == string.Empty)
    {
        throw new ArgumentException("Expense category cannot be empty.");
    }

    if (category.Trim().ToLower() != "transport" && category.Trim().ToLower() != "food" && category.Trim().ToLower() != "entertainment" && category.Trim().ToLower() != "bills" && category.Trim().ToLower() != "other")
    {
        throw new ArgumentException("Expense category must be 'transport', 'food', 'entertainment', 'bills', or 'other'");
    }

    List<ExpenseItem> expenseItems;

    if (!File.Exists(FilePath) || new FileInfo(FilePath).Length == 0)
    {
        throw new ArgumentException("No Expenses found");
    }
    else
    {
        string json = File.ReadAllText(FilePath);
        expenseItems = JsonSerializer.Deserialize<List<ExpenseItem>>(json, jsonOptions) ?? [];
    }

    ExpenseItem expenseToUpdate = null!;

    foreach (var expenseItem in expenseItems)
    {
        if (expenseItem.Id == expenseId)
        {
            expenseToUpdate = expenseItem;
        }
    }

    if (expenseToUpdate == null)
    {
        throw new ArgumentException("Expense not found");
    }

    expenseToUpdate.Description = description;
    expenseToUpdate.Amount = amount;
    expenseToUpdate.Category = category;
    expenseToUpdate.UpdatedAt = DateTime.Now;

    File.WriteAllText(FilePath, JsonSerializer.Serialize(expenseItems, jsonOptions));
}

if (command == "delete")
{
    if (args.Length < 2)
    {
        throw new ArgumentException("Please provide an expense Id.");
    }

    var id = args[1];
    if (!int.TryParse(id, out int expenseId))
    {
        throw new ArgumentException("Invalid Id format");
    }

    List<ExpenseItem> expenseItems;

    if (!File.Exists(FilePath) || new FileInfo(FilePath).Length == 0)
    {
        throw new ArgumentException("No Expenses found");
    }
    else
    {
        string json = File.ReadAllText(FilePath);
        expenseItems = JsonSerializer.Deserialize<List<ExpenseItem>>(json, jsonOptions) ?? [];
    }

    ExpenseItem expenseToDelete = null!;

    foreach (var expenseItem in expenseItems)
    {
        if (expenseItem.Id == expenseId)
        {
            expenseToDelete = expenseItem;
        }
    }

    if (expenseToDelete == null)
    {
        throw new ArgumentException("Expense not found");
    }

    expenseItems.Remove(expenseToDelete);

    File.WriteAllText(FilePath, JsonSerializer.Serialize(expenseItems, jsonOptions));
}

if (command == "list")
{
    if (args.Length == 1)
    {

        List<ExpenseItem> expenseItems;

        if (!File.Exists(FilePath) || new FileInfo(FilePath).Length == 0)
        {
            throw new ArgumentException("No expenses found");
        }
        else
        {
            string json = File.ReadAllText(FilePath);
            expenseItems = JsonSerializer.Deserialize<List<ExpenseItem>>(json, jsonOptions) ?? [];
        }

        Console.WriteLine($"{"Id",-3} | {"Description",-30} | {"Amount",-10} | {"Category",-15} | {"Date"}");
        Console.WriteLine(new string('-', 110));

        foreach (var expenseItem in expenseItems)
        {
            Console.WriteLine($"{expenseItem.Id,-3} | {expenseItem.Description,-30} | {expenseItem.Amount,-10} | {expenseItem.Category,-15} | {expenseItem.Date}");
        }

    }

    if (args.Length > 1)
    {
        var categoryArg = args[1];
        if (args[1] != "--category")
        {
            throw new ArgumentException("Invalid command format, use --category");
        }

        if (args.Length < 3)
        {
            throw new ArgumentException("Please provide a category.");
        }

        var category = args[2];
        if (category.Trim().ToLower() != "transport" && category.Trim().ToLower() != "food" && category.Trim().ToLower() != "entertainment" && category.Trim().ToLower() != "bills" && category.Trim().ToLower() != "other")
        {
            throw new ArgumentException("Expense category must be 'transport', 'food', 'entertainment', 'bills', or 'other'");
        }

        List<ExpenseItem> expenseItems;

        if (!File.Exists(FilePath) || new FileInfo(FilePath).Length == 0)
        {
            throw new ArgumentException("No expenses found");
        }
        else
        {
            string json = File.ReadAllText(FilePath);
            expenseItems = JsonSerializer.Deserialize<List<ExpenseItem>>(json, jsonOptions) ?? [];
        }

        Console.WriteLine($"{"Id",-3} | {"Description",-30} | {"Amount",-10} | {"Category",-15} | {"Date"}");
        Console.WriteLine(new string('-', 110));

        foreach (var expenseItem in expenseItems)
        {
            if (expenseItem.Category?.Trim().ToLower() == category.Trim().ToLower())
            {
                Console.WriteLine($"{expenseItem.Id,-3} | {expenseItem.Description,-30} | {expenseItem.Amount,-10} | {expenseItem.Category,-15} | {expenseItem.Date}");
            }
        }

        File.WriteAllText(FilePath, JsonSerializer.Serialize(expenseItems, jsonOptions));
    }
}

if (command == "summary")
{
    if (args.Length == 1)
    {

        List<ExpenseItem> expenseItems;

        if (!File.Exists(FilePath) || new FileInfo(FilePath).Length == 0)
        {
            throw new ArgumentException("No expenses found");
        }
        else
        {
            string json = File.ReadAllText(FilePath);
            expenseItems = JsonSerializer.Deserialize<List<ExpenseItem>>(json, jsonOptions) ?? [];
        }

        decimal summary = 0;

        foreach (var expenseItem in expenseItems)
        {
            summary += expenseItem.Amount;
        }

        Console.WriteLine($"Total expenses: ${summary}");

    }

    if (args.Length > 1)
    {
        var monthArg = args[1];
        if (monthArg != "--month")
        {
            throw new ArgumentException("Invalid command format, use --month");
        }

        if (args.Length < 3)
        {
            throw new ArgumentException("Please provide a month.");
        }

        if (args.Length < 4 || args[3] != "--year")
        {
            throw new ArgumentException("Invalid command format, use --year");
        }

        if (args.Length < 5)
        {
            throw new ArgumentException("Please provide a year.");
        }

        List<ExpenseItem> expenseItems;

        if (!File.Exists(FilePath) || new FileInfo(FilePath).Length == 0)
        {
            throw new ArgumentException("No expenses found");
        }
        else
        {
            string json = File.ReadAllText(FilePath);
            expenseItems = JsonSerializer.Deserialize<List<ExpenseItem>>(json, jsonOptions) ?? [];
        }

        var monthInput = args[2];
        var yearInput = args[4];

        decimal summary = 0;
        bool anyFound = false;

        foreach (var expenseItem in expenseItems)
        {
            var date = DateTime.ParseExact(expenseItem.Date, "yyyy-MM-dd", null);

            if ((date.Month.ToString("D2") == monthInput || date.Month.ToString() == monthInput) && date.Year.ToString() == yearInput)
            {
                summary += expenseItem.Amount;
                anyFound = true;
            }
        }

        if (!anyFound)
        {
            throw new ArgumentException("No expenses found for the given month and year.");
        }

        string monthName = new DateTime(int.Parse(yearInput), int.Parse(monthInput), 1).ToString("MMMM");

        Console.WriteLine($"Total expenses for {monthName} {yearInput}: ${summary}");

        File.WriteAllText(FilePath, JsonSerializer.Serialize(expenseItems, jsonOptions));
    }
}

if (command == "set-budget")
{
    if (args.Length < 2)
    {
        throw new ArgumentException("Please provide a budget amount.");
    }

    if (!decimal.TryParse(args[1], out decimal budgetAmount) || budgetAmount <= 0)
    {
        throw new ArgumentException("Invalid budget amount format");
    }

    const string ConfigPath = "config.json";

    var config = new BudgetConfig
    {
        MonthlyBudget = budgetAmount
    };

    File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, jsonOptions));

    Console.WriteLine($"Monthly budget set to ${budgetAmount}");
}

if (command == "export")
{
    if (!File.Exists(FilePath) || new FileInfo(FilePath).Length == 0)
    {
        throw new ArgumentException("No expenses found to export.");
    }

    string json = File.ReadAllText(FilePath);
    List<ExpenseItem> expenseItems = JsonSerializer.Deserialize<List<ExpenseItem>>(json, jsonOptions) ?? [];

    string exportPath = "expenses.csv";

    using var writer = new StreamWriter(exportPath);

    // Write header
    writer.WriteLine("Id, Description, Amount, Category, Date");

    // Write each expense
    foreach (var expense in expenseItems)
    {
        writer.WriteLine($"{expense.Id}, " +
                         $"\"{expense.Description}\", " +
                         $"{expense.Amount}, " +
                         $"\"{expense.Category}\", " +
                         $"{expense.Date}");
    }

    Console.WriteLine($"Data successfully exported to {exportPath}");
}

if (command == "help")
{
    Console.WriteLine("Available commands:");
    Console.WriteLine("add [description] [amount] [category] [date]");
    Console.WriteLine("list [category]");
    Console.WriteLine("summary [--month [month] --year [year]]");
    Console.WriteLine("set-budget [amount]");
    Console.WriteLine("help");
}