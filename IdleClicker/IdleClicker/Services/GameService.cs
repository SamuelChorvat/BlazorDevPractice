using System.Text.Json;
using IdleClicker.Models;
using Microsoft.JSInterop;
using Timer = System.Timers.Timer;

namespace IdleClicker.Services;

public class GameService : IDisposable, IGameService
{
    public int Gold { get; private set; }
    public List<Unit> Units { get; private set; } = new();

    private readonly Timer _goldTimer;
    
    private readonly ILogger<GameService> _logger;
    private readonly IJSRuntime _js;
    
    public event Action? OnChange;

    public GameService(ILogger<GameService> logger, IJSRuntime js)
    {
        _logger = logger;
        _js = js;
        
        Units.Add(new Unit
        {
            Name = "Grunt",
            Cost = 10,
            GoldPerSecond = 1
        });
        Units.Add(new Unit
        {
            Name = "Footman",
            Cost = 25,
            GoldPerSecond = 5
        });
        Units.Add(new Unit
        {
            Name = "Archer",
            Cost = 50,
            GoldPerSecond = 15
        });
        Units.Add(new Unit
        {
            Name = "Knight",
            Cost = 500,
            GoldPerSecond = 40
        });
        Units.Add(new Unit
        {
            Name = "Catapult",
            Cost = 1000,
            GoldPerSecond = 100
        });
        Units.Add(new Unit
        {
            Name = "Sorceress",
            Cost = 2500,
            GoldPerSecond = 220
        });
        Units.Add(new Unit
        {
            Name = "Paladin",
            Cost = 5000,
            GoldPerSecond = 500
        });
        
        Units = Units.OrderBy(u => u.Cost).ToList();
        _goldTimer = new Timer(1000);
        _goldTimer.Elapsed += (_, _) => GeneratePassiveGold();
        _goldTimer.Start();
        OnChange += CheckUnlocks;
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
    
    public async Task SaveGame()
    {
        var saveData = new GameSaveData
        {
            Gold = Gold,
            Units = Units
        };

        var json = JsonSerializer.Serialize(saveData);
        await _js.InvokeVoidAsync("localStorageHelper.save", "IdleClickerSave", json);

        _logger.LogInformation("Game saved");
    }

    public async Task LoadGame()
    {
        var json = await _js.InvokeAsync<string>("localStorageHelper.load", "IdleClickerSave");

        if (!string.IsNullOrWhiteSpace(json))
        {
            var saveData = JsonSerializer.Deserialize<GameSaveData>(json);
            if (saveData != null)
            {
                Gold = saveData.Gold;
                Units = saveData.Units;
                _logger.LogInformation("Game loaded");
                NotifyStateChanged();
            }
        }
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
    
    private void CheckUnlocks()
    {
        foreach (var unit in Units.Where(unit => !unit.IsUnlocked && Gold >= unit.Cost))
        {
            unit.IsUnlocked = true;
            _logger.LogInformation("{Unit} unlocked!", unit.Name);
        }
    }
}