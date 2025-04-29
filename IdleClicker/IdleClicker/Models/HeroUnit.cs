using IdleClicker.Services;

namespace IdleClicker.Models;

public class HeroUnit : Unit
{
    public bool BonusApplied { get; set; }
    public Action<GameService>? BonusEffect { get; set; }

    public override bool CanHire(int currentGold)
    {
        return base.CanHire(currentGold) && Quantity == 0;
    }
}