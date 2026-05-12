namespace StateMachineLibrary.Core.Definitions;

/// <summary>History restoration strategy for a hierarchy composite state.</summary>
public enum HistoryMode
{
    None = 0,
    Shallow = 1,
    Deep = 2
}