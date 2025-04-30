namespace IdleClicker.Models;

public class FloatingGoldEffect
{
    public string Text { get; init; } = "";
    public Guid Id { get; } = Guid.NewGuid();
    public string InlineStyle { get; init; } = "";
    public string ExtraClass { get; init; } = "";
}