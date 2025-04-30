using IdleClicker.Components.Shared;
using IdleClicker.Models;
using IdleClicker.Services;
using Microsoft.AspNetCore.Components;

namespace IdleClicker.Components.Units;

public partial class HeroUnitCard : BaseUnitCard
{
    [Inject] private IToastService ToastService { get; set; } = null!;

    protected override void Hire()
    {
        if (Unit is HeroUnit {Quantity: 0} hero)
        {
            GameService.HireUnit(hero);
            ToastService.Show($"{hero.Name} has been recruited! {hero.Description}");
        }
        else
        {
            GameService.HireUnit(Unit);
        }
    }
}