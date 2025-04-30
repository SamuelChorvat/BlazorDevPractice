using System.Text.Json;
using IdleClicker.Models;
using Microsoft.JSInterop;
using Timer = System.Timers.Timer;

namespace IdleClicker.Services;

public class GameService : IDisposable, IGameService
{
    public int Gold { get; private set; }
    public List<Unit> Units { get; } = [];

    private readonly Timer _goldTimer;
    
    private readonly ILogger<GameService> _logger;
    private readonly IJSRuntime _js;
    
    public event Action? OnChange;
    public event Action<int>? OnGoldEarned;

    public GameService(ILogger<GameService> logger, IJSRuntime js)
    {
        _logger = logger;
        _js = js;
        
        Units.Add(new Unit
        {
            Name = "Grunt",
            Cost = 10,
            GoldPerSecond = 1,
            IconClass = "bi-emoji-angry"
        });
        Units.Add(new Unit
        {
            Name = "Footman",
            Cost = 50,
            GoldPerSecond = 5,
            IconClass = "bi-shield-shaded"
        });
        Units.Add(new Unit
        {
            Name = "Archer",
            Cost = 150,
            GoldPerSecond = 15,
            IconClass = "bi-bullseye"
        });
        Units.Add(new Unit
        {
            Name = "Knight",
            Cost = 500,
            GoldPerSecond = 50,
            IconClass = "bi-shield-fill"
        });
        Units.Add(new Unit
        {
            Name = "Catapult",
            Cost = 1000,
            GoldPerSecond = 100,
            IconClass = "bi-gear-fill"
        });
        Units.Add(new Unit
        {
            Name = "Sorceress",
            Cost = 2500,
            GoldPerSecond = 250,
            IconClass = "bi-stars"
        });
        
        
        Units.Add(new HeroUnit
        {
            Name = "Uther",
            Cost = 50000,
            GoldPerSecond = 500,
            IconClass = "bi-lightning-charge",
            Description = "Increases Footman Gold/sec by 25%",
            BonusEffect = service =>
            {
                foreach (var unit in service.Units.Where(x => x.Name == "Footman"))
                {
                    unit.GoldPerSecond = (int)(unit.GoldPerSecond * 1.25);
                }
            }
        });
        Units.Add(new HeroUnit
        {
            Name = "Sylvanas",
            Cost = 120000,
            GoldPerSecond = 1200,
            IconClass = "bi-arrow-through-heart",
            Description = "Increases Archer Gold/sec by 25%",
            BonusEffect = service =>
            {
                var archer = service.Units.FirstOrDefault(u => u.Name == "Archer");
                if (archer != null)
                {
                    archer.GoldPerSecond = (int)(archer.GoldPerSecond * 1.25);
                }
            }
        });
        Units.Add(new HeroUnit
        {
            Name = "Jaina",
            Cost = 200000,
            GoldPerSecond = 2500,
            IconClass = "bi-book",
            Description = "Increases Sorceress Gold/sec by 25%",
            BonusEffect = service =>
            {
                var sorceress = service.Units.FirstOrDefault(u => u.Name == "Sorceress");
                if (sorceress != null)
                {
                    sorceress.GoldPerSecond = (int)(sorceress.GoldPerSecond * 1.25);
                }
            }
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
        OnGoldEarned?.Invoke(1);
    }

    public void HireUnit(Unit unit)
    {
        if (Gold < unit.Cost) return;
        if (unit is HeroUnit hero && hero.Quantity > 0) return;
        
        Gold -= unit.Cost;
        unit.Quantity++;
        if (unit is not HeroUnit)
        {
            unit.Cost = (int)(unit.Cost * 1.15);
        }
        
        if (unit is HeroUnit heroUnit && !heroUnit.BonusApplied && heroUnit.BonusEffect != null)
        {
            heroUnit.BonusEffect(this);
            heroUnit.BonusApplied = true;
            _logger.LogInformation("{HeroUnitName}\'s bonus applied!", heroUnit.Name);
        }
        
        NotifyStateChanged();
    }
    
    public async Task SaveGame()
    {
        var saveData = new GameSaveData
        {
            Gold = Gold,
            UnitsProgress = Units.Select(u => new UnitProgress
            {
                Name = u.Name,
                Quantity = u.Quantity,
                HeroBonusApplied = u is HeroUnit {BonusApplied: true}
            }).ToList()
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
                foreach (var savedUnit in saveData.UnitsProgress)
                {
                    var existingUnit = Units.FirstOrDefault(u => u.Name == savedUnit.Name);
                    if (existingUnit != null)
                    {
                        existingUnit.Quantity = savedUnit.Quantity;
                        if (existingUnit is HeroUnit hero)
                        {
                            hero.BonusApplied = savedUnit.HeroBonusApplied;
                        }
                    }
                }
                
                foreach (var unit in Units.OfType<HeroUnit>())
                {
                    if (unit.Quantity <= 0 || !unit.BonusApplied || unit.BonusEffect == null) continue;
                    unit.BonusEffect(this);
                    _logger.LogInformation("{UnitName}\'s bonus reapplied after load", unit.Name);
                }

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
        if (totalPassiveGold > 0)
        {
            OnGoldEarned?.Invoke(totalPassiveGold);
        }
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