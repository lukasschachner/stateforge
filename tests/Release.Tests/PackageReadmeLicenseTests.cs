using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class PackageReadmeLicenseTests
{
    [Fact]
    public void RepositoryReadmeAndLicenseExistAndArePackConfigured()
    {
        Assert.True(File.Exists(ProjectPaths.FullPath("README.md")));
        Assert.True(File.Exists(ProjectPaths.FullPath("LICENSE")));
        var props = ProjectFileAssertions.LoadDirectoryBuildProps();
        Assert.Equal("README.md", ProjectFileAssertions.RequiredProperty(props, "PackageReadmeFile"));
        Assert.Equal("LICENSE", ProjectFileAssertions.RequiredProperty(props, "PackageLicenseFile"));
    }

    [Fact]
    public void XmlDocsSymbolsAndSourcePackagingAreEnabled()
    {
        var props = ProjectFileAssertions.LoadDirectoryBuildProps();
        Assert.Equal("true", ProjectFileAssertions.RequiredProperty(props, "GenerateDocumentationFile"));
        Assert.Equal("true", ProjectFileAssertions.RequiredProperty(props, "IncludeSymbols"));
        Assert.Equal("snupkg", ProjectFileAssertions.RequiredProperty(props, "SymbolPackageFormat"));
        Assert.Equal("true", ProjectFileAssertions.RequiredProperty(props, "IncludeSource"));
    }
}