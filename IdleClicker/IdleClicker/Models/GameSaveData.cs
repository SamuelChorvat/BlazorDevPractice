namespace IdleClicker.Models;

public class GameSaveData
{
    public int Gold { get; set; }
    public List<UnitProgress> UnitsProgress { get; set; } = new();
}

public class UnitProgress
{
    public required string Name { get; set; }
    public int Quantity { get; set; }
}