# Release Tests

This project validates NuGet package release readiness.

## Scaffolding/build check

```bash
dotnet build tests/Release.Tests/Release.Tests.csproj --configuration Release
```

## Package validation command

Create artifacts before package-content inspection:

```bash
dotnet pack StateForge.slnx --configuration Release --output artifacts/packages
```

Expected artifact inventory:

```text
artifacts/packages/
├── StateForge.Core.<version>.nupkg
├── StateForge.Core.<version>.snupkg
├── StateForge.SourceGenerators.<version>.nupkg
├── StateForge.Persistence.<version>.nupkg
├── StateForge.Persistence.<version>.snupkg
├── StateForge.OpenTelemetry.<version>.nupkg
├── StateForge.OpenTelemetry.<version>.snupkg
├── StateForge.Visualization.Mermaid.<version>.nupkg
├── StateForge.Visualization.Mermaid.<version>.snupkg
├── StateForge.Visualization.Graphviz.<version>.nupkg
├── StateForge.Visualization.Graphviz.<version>.snupkg
├── StateForge.Visualization.PlantUML.<version>.nupkg
└── StateForge.Visualization.PlantUML.<version>.snupkg
```

Package tests record zero unintended files and zero disallowed dependencies for all packable package ids. When artifacts
are absent, archive-inspection tests are inert so normal `dotnet test` can run before pack.

## Public API mutation check

To validate regression protection, temporarily remove or rename a public member in Core, SourceGenerators, Persistence,
OpenTelemetry, or a Visualization package and run:

```bash
dotnet test --project tests/Release.Tests/Release.Tests.csproj --configuration Release --filter PublicApi
```

The corresponding snapshot test must fail until the change is reviewed and the approved snapshot is intentionally
updated.

## Release script behavior

`eng/validate-release.sh` and `eng/validate-release.ps1` run restore, build, test, format verification, and pack in
order. They do not publish packages.

## Final validation outcome

Latest local validation completed with zero unintended files and zero disallowed dependencies reported by release tests.
Produced artifacts:

```text
StateForge.Core.0.1.0-rc.1.nupkg
StateForge.Core.0.1.0-rc.1.snupkg
StateForge.SourceGenerators.0.1.0-rc.1.nupkg
StateForge.Persistence.0.1.0-rc.1.nupkg
StateForge.Persistence.0.1.0-rc.1.snupkg
StateForge.OpenTelemetry.0.1.0-rc.1.nupkg
StateForge.OpenTelemetry.0.1.0-rc.1.snupkg
StateForge.Visualization.Mermaid.0.1.0-rc.1.nupkg
StateForge.Visualization.Graphviz.0.1.0-rc.1.nupkg
StateForge.Visualization.PlantUML.0.1.0-rc.1.nupkg
```

The SourceGenerators analyzer package is packed as an analyzer `.nupkg` without a separate `.snupkg`; Core, Persistence,
OpenTelemetry, and Visualization renderer packages emit symbol packages.

## Completion Transition Validation

Completion-transition release validation should run:

```bash
dotnet test --solution StateForge.slnx --filter Completion
dotnet test --solution StateForge.slnx --filter CorePublicApiSnapshotTests
dotnet format StateForge.slnx --verify-no-changes
dotnet pack StateForge.slnx --configuration Release --output artifacts/packages
```

Expected outcome: completion runtime, validation, graph export, visualization, and public API snapshot checks pass with no new Core runtime dependencies.

## Application integration adapter packages

Release tests include DependencyInjection and Logging in packable project discovery, public API snapshot validation, package metadata validation, and package-boundary validation.
