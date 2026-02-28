using System;

namespace ExpenseTrackerCli;

public class ExpenseItem
{
    public int Id { get; set; }
    public string Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

}
