# Parallel Region Builder Release Notes

Fluent region builders are an additive Core API surface for parallel composites.

Release validation should confirm:

- existing `Region(...)` and `ParallelRegion(...)` overloads remain source-compatible;
- new block-style region declarations populate the same immutable definition model as old-style declarations;
- validation reports blank names, duplicate membership, and state/list membership drift without changing runtime semantics;
- introspection and graph export expose equivalent region metadata for old-style, new-style, and valid mixed definitions;
- `tests/Release.Tests/PublicApi/Core.approved.txt` was reviewed for the additive `ParallelRegionDefinitionBuilder<TState,TEvent>` surface.
