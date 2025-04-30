namespace IdleClicker.Services;

public interface IToastService
{
    void Show(string message);
    event Action<string>? OnShow;
}