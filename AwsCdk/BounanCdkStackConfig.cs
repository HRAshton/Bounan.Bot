using System;

namespace Bounan.Downloader.AwsCdk;

public class BounanCdkStackConfig
{
    public required string AlertEmail { get; init; }

    public required string LoanApiToken { get; init; }

    public required string TelegramBotToken { get; init; }

    public required string GetAnimeFunctionName { get; init; }

    public required string NotificationQueueArn { get; init; }

    public required bool? RegisterWebhook { get; init; }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(AlertEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(LoanApiToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(TelegramBotToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(GetAnimeFunctionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(NotificationQueueArn);
        ArgumentNullException.ThrowIfNull(RegisterWebhook);
    }
}