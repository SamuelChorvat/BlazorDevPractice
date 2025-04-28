namespace IdleClicker.Models;

public class GameSaveData
{
    public int Gold { get; set; }
    public List<Unit> Units { get; set; } = new();
}