namespace Bounan.Bot.BusinessLogic.Configs;

public class RetryPolicyConfig
{
    public const string SectionName = "RetryPolicy";

    public required int Retries { get; init; } = 3;
}