namespace IdleClicker.Models;

public class GameSaveData
{
    public int Gold { get; init; }
    public List<UnitProgress> UnitsProgress { get; init; } = [];
}

public class UnitProgress
{
    public required string Name { get; init; }
    public int Quantity { get; init; }
    public bool HeroBonusApplied { get; set; }
}