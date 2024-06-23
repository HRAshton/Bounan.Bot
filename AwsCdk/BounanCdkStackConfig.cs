using System;
using System.Globalization;

namespace Bounan.Downloader.AwsCdk;

public class BounanCdkStackConfig
{
    public required string AlertEmail { get; init; }

    public required string LoanApiToken { get; init; }

    public required string TelegramBotToken { get; init; }

    public required string TelegramBotVideoChatId { get; init; }

    public required string TelegramBotForwardingChatId { get; init; }

    public required string TelegramBotPublisherGroup { get; init; }

    public required string GetAnimeFunctionName { get; init; }

    public required string VideoDownloadedTopicArn { get; init; }

    public int WarmupTimeoutMinutes { get; init; } = 5;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(AlertEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(LoanApiToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(TelegramBotToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(GetAnimeFunctionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(VideoDownloadedTopicArn);
        ArgumentException.ThrowIfNullOrWhiteSpace(TelegramBotPublisherGroup);
        if (TelegramBotPublisherGroup[0] != '@')
        {
            throw new ArgumentException("TelegramBotPublisherGroup must start with '@'");
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            long.Parse(TelegramBotVideoChatId, NumberFormatInfo.InvariantInfo),
            0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            long.Parse(TelegramBotForwardingChatId, NumberFormatInfo.InvariantInfo),
            0);
    }
}