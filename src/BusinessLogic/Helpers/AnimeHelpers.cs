namespace Bounan.Bot.BusinessLogic.Helpers;

public static class AnimeHelpers
{
    public static string DubToKey(string dub)
    {
        return dub.ToLower().Replace(" ", "_");
    }
}