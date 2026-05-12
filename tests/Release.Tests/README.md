# Release Tests

This project validates NuGet package release readiness.

## Scaffolding/build check

```bash
dotnet build tests/Release.Tests/Release.Tests.csproj --configuration Release
```

## Package validation command

Create artifacts before package-content inspection:

```bash
dotnet pack StateMachineLibrary.sln --configuration Release --output artifacts/packages
```

Expected artifact inventory:

```text
artifacts/packages/
├── StateMachineLibrary.Core.<version>.nupkg
├── StateMachineLibrary.Core.<version>.snupkg
├── StateMachineLibrary.SourceGenerators.<version>.nupkg
├── StateMachineLibrary.Persistence.<version>.nupkg
├── StateMachineLibrary.Persistence.<version>.snupkg
├── StateMachineLibrary.OpenTelemetry.<version>.nupkg
├── StateMachineLibrary.OpenTelemetry.<version>.snupkg
├── StateMachineLibrary.Visualization.Mermaid.<version>.nupkg
├── StateMachineLibrary.Visualization.Mermaid.<version>.snupkg
├── StateMachineLibrary.Visualization.Graphviz.<version>.nupkg
├── StateMachineLibrary.Visualization.Graphviz.<version>.snupkg
├── StateMachineLibrary.Visualization.PlantUML.<version>.nupkg
└── StateMachineLibrary.Visualization.PlantUML.<version>.snupkg
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
StateMachineLibrary.Core.0.1.0-rc.1.nupkg
StateMachineLibrary.Core.0.1.0-rc.1.snupkg
StateMachineLibrary.SourceGenerators.0.1.0-rc.1.nupkg
StateMachineLibrary.Persistence.0.1.0-rc.1.nupkg
StateMachineLibrary.Persistence.0.1.0-rc.1.snupkg
StateMachineLibrary.OpenTelemetry.0.1.0-rc.1.nupkg
StateMachineLibrary.OpenTelemetry.0.1.0-rc.1.snupkg
StateMachineLibrary.Visualization.Mermaid.0.1.0-rc.1.nupkg
StateMachineLibrary.Visualization.Graphviz.0.1.0-rc.1.nupkg
StateMachineLibrary.Visualization.PlantUML.0.1.0-rc.1.nupkg
```

The SourceGenerators analyzer package is packed as an analyzer `.nupkg` without a separate `.snupkg`; Core, Persistence,
OpenTelemetry, and Visualization renderer packages emit symbol packages.

## Completion Transition Validation

Completion-transition release validation should run:

```bash
dotnet test --solution StateMachineLibrary.sln --filter Completion
dotnet test --solution StateMachineLibrary.sln --filter CorePublicApiSnapshotTests
dotnet format StateMachineLibrary.sln --verify-no-changes
dotnet pack StateMachineLibrary.sln --configuration Release --output artifacts/packages
```

Expected outcome: completion runtime, validation, graph export, visualization, and public API snapshot checks pass with no new Core runtime dependencies.
