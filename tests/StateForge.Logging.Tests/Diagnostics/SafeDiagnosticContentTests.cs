using StateForge.Logging.Configuration;
using StateForge.Logging.Diagnostics;

namespace StateForge.Logging.Tests.Diagnostics;

public sealed class SafeDiagnosticContentTests
{
    [Fact]
    public void RedactsSecretLikeValuesAndTruncatesLongValues()
    {
        Assert.Equal("[redacted]", SafeDiagnosticFormatter.SafeValue("password=abc"));
        var safe = SafeDiagnosticFormatter.SafeValue(new string('x', 200), new StateMachineLoggingOptions { MaxMetadataValueLength = 32 });
        Assert.EndsWith("…", safe);
    }
}
