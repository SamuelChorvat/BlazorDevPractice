using IdleClicker.Models;

namespace IdleClicker.Services;

public interface IGameService
{
    int Gold { get; }
    int PassiveGoldPerSecond { get; }
    int ClickComboCount { get;}
    List<Unit> Units { get; }
    event Action? OnChange;
    event Action<int>? OnGoldEarned;
    event Action<int>? OnManualGoldEarned;
    void GatherGold();
    void HireUnit(Unit unit);
    Task SaveGame();
    Task LoadGame();
}