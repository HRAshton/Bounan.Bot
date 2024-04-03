using Bounan.Bot.BusinessLogic.CommandDto;

namespace Bounan.Bot.BusinessLogic.Helpers;

internal static class CommandConvert
{
    public static string SerializeCommand(ICommandDto command)
    {
        var propValues = command.GetType().GetProperties()
            .Select(x => x.GetValue(command)?.ToString() ?? throw new InvalidOperationException())
            .ToArray();

        if (propValues.Length == 0 || propValues.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("A command must be fully initialized.");
        }

        return string.Join(' ', propValues);
    }

    public static TCommandDto? DeserializeCommand<TCommandDto>(string text)
        where TCommandDto : class
    {
        var properties = typeof(TCommandDto).GetProperties();
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (properties.Length != parts.Length)
        {
            return null;
        }

        try
        {
            var instance = Activator.CreateInstance<TCommandDto>();

            // Skip the first property, it's the command name
            for (var i = 1; i < properties.Length; i++)
            {
                var value = Convert.ChangeType(parts[i], properties[i].PropertyType);
                properties[i].SetValue(instance, value);
            }

            return instance;
        }
        catch
        {
            return null;
        }
    }
}