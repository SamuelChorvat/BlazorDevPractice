using IdleClicker.Models;
using IdleClicker.Services;
using Microsoft.AspNetCore.Components;

namespace IdleClicker.Components.Shared;

public abstract class BaseUnitCard : ComponentBase
{
    [Parameter] 
    public Unit Unit { get; set; } = null!;
    [Inject] 
    protected IGameService GameService { get; set; } = null!;

    protected virtual void Hire()
    {
        GameService.HireUnit(Unit);
    }
}