﻿using System;

namespace Bounan.Downloader.AwsCdk;

public class BounanCdkStackConfig
{
    public required string AlertEmail { get; init; }

    public required string LoanApiToken { get; init; }

    public required string TelegramBotToken { get; init; }

    public required string GetAnimeFunctionName { get; init; }

    public required string NotificationQueueArn { get; init; }

    public int WarmupTimeoutMinutes { get; init; } = 5;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(AlertEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(LoanApiToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(TelegramBotToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(GetAnimeFunctionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(NotificationQueueArn);
    }
}