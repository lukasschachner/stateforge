namespace StateForge.SourceGenerators.Tests;

public sealed class AttributeNameConflictTests
{
    [Fact]
    public void UnrelatedAttributesWithGeneratorLikeNamesAreIgnored()
    {
        var source = """
                     namespace Other
                     {
                         public sealed class StateMachineAttribute : System.Attribute
                         {
                             public StateMachineAttribute(System.Type stateType, System.Type eventType) { }
                         }
                     }

                     public enum S { A, B }
                     public enum E { Go }

                     [Other.StateMachine(typeof(S), typeof(E))]
                     public static partial class NotAGeneratedMachine { }
                     """;
        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.DoesNotContain(result.GeneratedHintNames,
            h => h.EndsWith(".StateMachine.g.cs", StringComparison.Ordinal));
    }
}