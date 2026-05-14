#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

rm -rf artifacts/packages
mkdir -p artifacts/packages

solution_file="StateForge.slnx"
if [[ ! -f "$solution_file" && -f "StateForge.sln" ]]; then
  solution_file="StateForge.sln"
fi

dotnet restore "$solution_file"
dotnet build "$solution_file" --configuration Release --no-restore
dotnet test --solution "$solution_file" --configuration Release --no-build
hierarchy_output="$(dotnet run --project samples/Core.HierarchySample/Core.HierarchySample.csproj --configuration Release --no-build)"
printf '%s\n' "$hierarchy_output"
grep -q "History restored path: Reviewing -> LegalReview" <<< "$hierarchy_output"
grep -q "Active snapshot:" <<< "$hierarchy_output"
grep -q "Snapshot restored leaf:" <<< "$hierarchy_output"
grep -q "Parallel regions:" <<< "$hierarchy_output"
grep -q "Parallel active snapshot:" <<< "$hierarchy_output"
grep -q "Parallel history restored regions:" <<< "$hierarchy_output"
grep -q "Parallel history snapshots:" <<< "$hierarchy_output"
graph_output="$(dotnet run --project samples/Graph.IntrospectionSample/Graph.IntrospectionSample.csproj --configuration Release --no-build)"
printf '%s\n' "$graph_output"
grep -q "Parallel history definition:" <<< "$graph_output"
grep -q "Active snapshot kind:" <<< "$graph_output"
grep -q "Introspection snapshot kind:" <<< "$graph_output"
grep -q "Recorded parallel history:" <<< "$graph_output"
dotnet format "$solution_file" --verify-no-changes
dotnet pack "$solution_file" --configuration Release --no-build --output artifacts/packages

echo "Release validation completed. Artifacts are in artifacts/packages. No publish step was run."
