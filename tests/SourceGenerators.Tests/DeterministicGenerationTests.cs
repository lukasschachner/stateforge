namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class DeterministicGenerationTests
{
    [Fact]
    public void UnchangedAttributeDeclarationsGenerateByteStableSource()
    {
        var first = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        var second = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        GeneratorTestHost.AssertCompiles(first);
        GeneratorTestHost.AssertCompiles(second);
        Assert.Equal(first.GeneratedSource, second.GeneratedSource);
    }

    [Fact]
    public void AdvancedRegionDeclarationsGenerateByteStableSource()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { Operational, A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Operational, IsParallelComposite = true)]
                     [Region(S.Operational, "One", S.A, IsInitial = true)]
                     [Region(S.Operational, "One", S.B, IsTerminal = true)]
                     public static partial class M { }
                     """;

        var first = GeneratorTestHost.Run(source);
        var second = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(first);
        GeneratorTestHost.AssertCompiles(second);
        Assert.Equal(first.GeneratedSource, second.GeneratedSource);
        var expectedRegion = "machine.ParallelComposite(S.Operational).Region(\"One\", S.A, new global::S[] { S.B }, new global::S[] { S.B });";
        Assert.Contains(expectedRegion, first.GeneratedSource);
        Assert.Contains(expectedRegion, second.GeneratedSource);
        Assert.True(first.GeneratedSource.IndexOf(expectedRegion, StringComparison.Ordinal) >= 0);
    }
}
