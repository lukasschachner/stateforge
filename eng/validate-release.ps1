$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

if (Test-Path artifacts/packages) { Remove-Item -Recurse -Force artifacts/packages }
New-Item -ItemType Directory -Force artifacts/packages | Out-Null

dotnet restore StateMachineLibrary.sln
dotnet build StateMachineLibrary.sln --configuration Release --no-restore
dotnet test --solution StateMachineLibrary.sln --configuration Release --no-build
$HierarchyOutput = dotnet run --project samples/Core.HierarchySample/Core.HierarchySample.csproj --configuration Release --no-build
$HierarchyOutput | Write-Host
$HierarchyOutputText = $HierarchyOutput -join [Environment]::NewLine
if ($HierarchyOutputText -notmatch 'History restored path: Reviewing -> LegalReview') { throw 'Hierarchy sample did not demonstrate history restoration.' }
if ($HierarchyOutputText -notmatch 'Active snapshot:') { throw 'Hierarchy sample did not demonstrate active snapshot capture.' }
if ($HierarchyOutputText -notmatch 'Snapshot restored leaf:') { throw 'Hierarchy sample did not demonstrate active snapshot restore.' }
if ($HierarchyOutputText -notmatch 'Parallel regions:') { throw 'Hierarchy sample did not demonstrate parallel regions.' }
if ($HierarchyOutputText -notmatch 'Parallel active snapshot:') { throw 'Hierarchy sample did not demonstrate parallel active snapshots.' }
if ($HierarchyOutputText -notmatch 'Parallel history restored regions:') { throw 'Hierarchy sample did not demonstrate parallel history restoration.' }
if ($HierarchyOutputText -notmatch 'Parallel history snapshots:') { throw 'Hierarchy sample did not print parallel history snapshots.' }
$GraphOutput = dotnet run --project samples/Graph.IntrospectionSample/Graph.IntrospectionSample.csproj --configuration Release --no-build
$GraphOutput | Write-Host
$GraphOutputText = $GraphOutput -join [Environment]::NewLine
if ($GraphOutputText -notmatch 'Parallel history definition:') { throw 'Graph introspection sample did not print parallel history metadata.' }
if ($GraphOutputText -notmatch 'Active snapshot kind:') { throw 'Graph introspection sample did not print active snapshot metadata.' }
if ($GraphOutputText -notmatch 'Introspection snapshot kind:') { throw 'Graph introspection sample did not print snapshot vocabulary metadata.' }
if ($GraphOutputText -notmatch 'Recorded parallel history:') { throw 'Graph introspection sample did not print recorded parallel history.' }
dotnet format StateMachineLibrary.sln --verify-no-changes
dotnet pack StateMachineLibrary.sln --configuration Release --no-build --output artifacts/packages

Write-Host "Release validation completed. Artifacts are in artifacts/packages. No publish step was run."
