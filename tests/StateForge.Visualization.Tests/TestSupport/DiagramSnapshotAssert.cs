namespace StateForge.Visualization.Tests.TestSupport;

internal static class DiagramSnapshotAssert
{
    public static void MatchesApproved(string approvedFileName, string actual)
    {
        var approvedPath = SnapshotPath(approvedFileName);
        var normalized = TextNormalization.NormalizeForSnapshot(actual);

        if (Environment.GetEnvironmentVariable("UPDATE_VISUALIZATION_SNAPSHOTS") == "1" || !File.Exists(approvedPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(approvedPath)!);
            File.WriteAllText(approvedPath, normalized);
        }

        var expected = TextNormalization.NormalizeForSnapshot(File.ReadAllText(approvedPath));
        Assert.True(expected == normalized,
            $"Snapshot mismatch for {approvedFileName}. Update only after reviewing deterministic output changes.\n--- approved\n{expected}\n--- actual\n{normalized}");
    }

    private static string SnapshotPath(string fileName)
    {
        return Path.Combine(FindRepositoryRoot(), "tests", "StateForge.Visualization.Tests", "Snapshots", fileName);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "StateForge.sln"))) return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}