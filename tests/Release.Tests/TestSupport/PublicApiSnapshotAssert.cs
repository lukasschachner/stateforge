namespace Release.Tests.TestSupport;

internal static class PublicApiSnapshotAssert
{
    public static void MatchesApproved(PackableProject project)
    {
        var actual = PublicApiSnapshotGenerator.Generate(project.MarkerType.Assembly);
        var path = ProjectPaths.FullPath(project.ApprovedSnapshotPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (Environment.GetEnvironmentVariable("UPDATE_PUBLIC_API_SNAPSHOTS") == "1" || !File.Exists(path))
            File.WriteAllText(path, actual);
        var expected = File.ReadAllText(path).Replace("\r\n", "\n");
        Assert.True(expected == actual,
            $"Public API snapshot mismatch for {project.Name}. Update {project.ApprovedSnapshotPath} only after reviewing the public contract change.\n--- approved\n{expected}\n--- actual\n{actual}");
    }
}