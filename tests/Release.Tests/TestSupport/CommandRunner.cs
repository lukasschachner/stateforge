using System.Diagnostics;

namespace Release.Tests.TestSupport;

internal static class CommandRunner
{
    public static string Run(string fileName, string arguments, int timeoutSeconds = 60)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = ProjectPaths.RepositoryRoot,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(timeoutSeconds * 1000))
        {
            try
            {
                process.Kill(true);
            }
            catch
            {
            }

            Assert.Fail($"Command timed out: {fileName} {arguments}");
        }

        var output = outputTask.GetAwaiter().GetResult() + errorTask.GetAwaiter().GetResult();
        Assert.True(process.ExitCode == 0, $"Command failed ({process.ExitCode}): {fileName} {arguments}\n{output}");
        return output;
    }
}