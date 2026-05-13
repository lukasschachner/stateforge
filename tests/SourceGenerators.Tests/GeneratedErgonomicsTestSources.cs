namespace StateMachineLibrary.SourceGenerators.Tests;

internal static class GeneratedErgonomicsTestSources
{
    public const string SimpleMachine = """
                                        using StateMachineLibrary.SourceGeneration;
                                        public enum S { A, B }
                                        public enum E { Go }
                                        [StateMachine(typeof(S), typeof(E))]
                                        [State(S.A)]
                                        [State(S.B, IsTerminal = true)]
                                        [Event(E.Go)]
                                        [Transition(S.A, E.Go, S.B)]
                                        public static partial class M;
                                        """;
}
