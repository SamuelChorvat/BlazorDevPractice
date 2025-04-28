using IdleClicker.Models;
using Timer = System.Timers.Timer;

namespace IdleClicker.Services;

public class GameService : IDisposable, IGameService
{
    public int Gold { get; private set; }
    public List<Unit> Units { get; private set; } = new();

    private readonly Timer _goldTimer;
    
    private readonly ILogger<GameService> _logger;

    public event Action? OnChange;

    public GameService(ILogger<GameService> logger)
    {
        _logger = logger;
        
        Units.Add(new Unit
        {
            Name = "Grunt",
            Cost = 10,
            GoldPerSecond = 1
        });

        _goldTimer = new Timer(1000);
        _goldTimer.Elapsed += (s, e) => GeneratePassiveGold();
        _goldTimer.Start();
    }

    public void GatherGold()
    {
        Gold += 1;
        _logger.LogInformation("GatherGold called. New Gold Amount: {Gold}", Gold);
        NotifyStateChanged();
    }

    public void HireUnit(Unit unit)
    {
        if (Gold < unit.Cost) return;
        Gold -= unit.Cost;
        unit.Quantity++;
        unit.Cost = (int)(unit.Cost * 1.15);
        NotifyStateChanged();
    }

    private void GeneratePassiveGold()
    {
        var totalPassiveGold = Units.Sum(u => u.GoldPerSecond * u.Quantity);
        Gold += totalPassiveGold;
        _logger.LogInformation("Passive gold generated: {Amount}. Total Gold: {Gold}", totalPassiveGold, Gold);
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }

    public void Dispose()
    {
        _goldTimer?.Dispose();
    }
}