namespace IdleClicker.Models;

public class HeroToastMessage
{
    public string Message { get; set; } = "";
    public Guid Id { get; set; } = Guid.NewGuid();
}
