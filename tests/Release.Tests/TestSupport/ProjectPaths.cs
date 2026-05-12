namespace Release.Tests.TestSupport;

internal static class ProjectPaths
{
    public static string RepositoryRoot { get; } = FindRepositoryRoot();
    public static string SolutionPath => Path.Combine(RepositoryRoot, "StateMachineLibrary.sln");
    public static string ArtifactsPackageDirectory => Path.Combine(RepositoryRoot, "artifacts", "packages");

    public static string FullPath(string relativePath)
    {
        return Path.Combine(RepositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    public static string ReadAllText(string relativePath)
    {
        return File.ReadAllText(FullPath(relativePath));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "StateMachineLibrary.sln"))) return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}