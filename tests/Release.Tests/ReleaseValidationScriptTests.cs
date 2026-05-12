using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class ReleaseValidationScriptTests
{
    [Fact]
    public void BashValidationScriptContainsReleaseCommandsInOrder()
    {
        var text = ProjectPaths.ReadAllText("eng/validate-release.sh");
        ReleaseValidationFacts.AssertCommandsInOrder(text);
        Assert.Contains("set -euo pipefail", text);
        Assert.Contains("Active snapshot:", text, StringComparison.Ordinal);
        Assert.Contains("Active snapshot kind:", text, StringComparison.Ordinal);
    }

    [Fact]
    public void PowerShellValidationScriptContainsReleaseCommandsInOrder()
    {
        var text = ProjectPaths.ReadAllText("eng/validate-release.ps1");
        ReleaseValidationFacts.AssertCommandsInOrder(text);
        Assert.Contains("$ErrorActionPreference = 'Stop'", text);
        Assert.Contains("Active snapshot:", text, StringComparison.Ordinal);
        Assert.Contains("Active snapshot kind:", text, StringComparison.Ordinal);
    }
}