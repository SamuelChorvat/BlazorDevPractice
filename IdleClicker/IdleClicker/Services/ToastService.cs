namespace IdleClicker.Services;

public class ToastService : IToastService
{
    public event Action<string>? OnShow;

    public void Show(string message)
    {
        OnShow?.Invoke(message);
    }
}
