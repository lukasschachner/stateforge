namespace Release.Tests.TestSupport;

internal static class ReleaseValidationFacts
{
    public static readonly string[] RequiredCategories = ["restore", "build", "test", "format", "pack"];

    public static readonly string[] OrderedCliCommands =
    [
        "dotnet restore StateMachineLibrary.sln",
        "dotnet build StateMachineLibrary.sln --configuration Release --no-restore",
        "dotnet test StateMachineLibrary.sln --configuration Release --no-build",
        "dotnet format StateMachineLibrary.sln --verify-no-changes",
        "dotnet pack StateMachineLibrary.sln --configuration Release --no-build --output artifacts/packages"
    ];

    public static void AssertCommandsInOrder(string text)
    {
        var position = -1;
        foreach (var command in OrderedCliCommands)
        {
            var next = text.IndexOf(command, StringComparison.Ordinal);
            Assert.True(next > position, $"Expected command in order: {command}");
            position = next;
        }
    }

    public static void AssertNoPublishCommand(string text)
    {
        Assert.DoesNotContain("dotnet nuget push", text, StringComparison.OrdinalIgnoreCase);
    }
}