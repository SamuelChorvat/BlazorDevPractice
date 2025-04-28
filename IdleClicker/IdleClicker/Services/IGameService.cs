using IdleClicker.Models;

namespace IdleClicker.Services;

public interface IGameService
{
    int Gold { get; }
    List<Unit> Units { get; }
    event Action? OnChange;
    void GatherGold();
    void HireUnit(Unit unit);

    Task SaveGame();
    Task LoadGame();
}