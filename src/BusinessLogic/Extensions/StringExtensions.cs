namespace Bounan.Bot.BusinessLogic.Extensions;

public static class StringExtensions
{
    public static string? Or(this string? value, string? defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}