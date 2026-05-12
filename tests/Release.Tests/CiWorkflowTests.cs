using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class CiWorkflowTests
{
    [Fact]
    public void CiRunsRequiredValidationCategoriesAndDoesNotPublish()
    {
        var text = ProjectPaths.ReadAllText(".github/workflows/ci.yml");
        foreach (var category in ReleaseValidationFacts.RequiredCategories)
            Assert.Contains(category, text, StringComparison.OrdinalIgnoreCase);
        foreach (var command in ReleaseValidationFacts.OrderedCliCommands)
            Assert.Contains(command, text, StringComparison.Ordinal);
        Assert.Contains("actions/upload-artifact", text, StringComparison.OrdinalIgnoreCase);
        ReleaseValidationFacts.AssertNoPublishCommand(text);
    }
}