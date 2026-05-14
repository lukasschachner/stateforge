namespace StateForge.SourceGenerators.Tests;

public sealed class SafeDiagnosticContentTests
{
    [Fact]
    public void DiagnosticMessagesDoNotContainEnvironmentSpecificContent()
    {
        var source = ValidationDiagnosticTestSources.Machine("""
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [State(S.C)]
                     [Event(E.Go)]
                     [Transition(S.A, E.Go, S.B)]
                     public static partial class M { }
                     """);

        var diagnostics = GeneratorTestHost.Run(source).GeneratorDiagnostics("SMG002").ToArray();
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "SMG002");
        foreach (var diagnostic in diagnostics)
        {
            var message = diagnostic.GetMessage();
            Assert.DoesNotContain(Environment.CurrentDirectory, message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetTempPath(), message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("System.Exception:", message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\n   at ", message, StringComparison.Ordinal);
        }
    }
}
