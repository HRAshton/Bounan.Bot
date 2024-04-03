using JetBrains.Annotations;

namespace Bounan.Bot.BusinessLogic.Configs;

public class ButtonsPagination
{
    public int Columns { get; [UsedImplicitly] init; } = 7;

    public int Rows { get; [UsedImplicitly] init; } = 3;
}