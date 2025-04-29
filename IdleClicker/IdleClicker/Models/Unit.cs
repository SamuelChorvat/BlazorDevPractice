namespace IdleClicker.Models;

public class Unit
{
    public string Name { get; set; } = string.Empty;
    public int Cost { get; set; }
    public int GoldPerSecond { get; set; }
    public int Quantity { get; set; }
    public bool IsUnlocked { get; set; }
    public string IconClass { get; init; } = "bi-question-circle";
}