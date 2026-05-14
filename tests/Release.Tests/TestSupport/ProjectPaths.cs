namespace Release.Tests.TestSupport;

internal static class ProjectPaths
{
    public static string RepositoryRoot { get; } = FindRepositoryRoot();
    public static string SolutionPath { get; } = FindSolutionPath(RepositoryRoot);
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
            if (File.Exists(Path.Combine(directory.FullName, "StateForge.slnx")) ||
                File.Exists(Path.Combine(directory.FullName, "StateForge.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }

    private static string FindSolutionPath(string repositoryRoot)
    {
        var slnx = Path.Combine(repositoryRoot, "StateForge.slnx");
        if (File.Exists(slnx)) return slnx;

        var sln = Path.Combine(repositoryRoot, "StateForge.sln");
        if (File.Exists(sln)) return sln;

        throw new FileNotFoundException("Could not locate StateForge solution file (.slnx or .sln).", repositoryRoot);
    }
}