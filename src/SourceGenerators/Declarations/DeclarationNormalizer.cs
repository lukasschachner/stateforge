namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class DeclarationNormalizer
{
    public static MachineDeclaration Normalize(MachineDeclaration declaration)
    {
        EventDeclarationNormalizer.EnsureTransitionEvents(declaration);
        NormalizeAdvancedDeclarations(declaration);
        return declaration;
    }

    private static void NormalizeAdvancedDeclarations(MachineDeclaration declaration)
    {
        foreach (var state in declaration.States.ToArray())
        {
            if (state.ParentStateKey is not null && state.ParentStateExpression is not null)
                EnsureState(declaration, state.ParentStateExpression, state.ParentStateKey, state.ParentLocation);

            if (state.InitialChildStateKey is not null && state.InitialChildExpression is not null)
            {
                var child = EnsureState(declaration, state.InitialChildExpression, state.InitialChildStateKey,
                    state.InitialChildLocation);
                child.ParentStateKey ??= state.IdentityKey;
                child.ParentStateExpression ??= state.ValueExpression;
                child.ParentLocation ??= state.InitialChildLocation;
                var composite = MergeComposite(declaration, state.IdentityKey, state.ValueExpression,
                    state.InitialChildLocation);
                composite.InitialChildStateKey = state.InitialChildStateKey;
                composite.InitialChildExpression = state.InitialChildExpression;
                composite.InitialChildLocation = state.InitialChildLocation;
            }

            if (state.HistoryMode != DeclaredHistoryMode.None || state.HistoryFallbackStateKey is not null)
            {
                var composite = MergeComposite(declaration, state.IdentityKey, state.ValueExpression,
                    state.HistoryLocation);
                composite.HistoryMode = state.HistoryMode;
                composite.HistoryFallbackStateKey = state.HistoryFallbackStateKey;
                composite.HistoryFallbackExpression = state.HistoryFallbackExpression;
                composite.HistoryLocation = state.HistoryLocation;
                if (state.HistoryFallbackStateKey is not null && state.HistoryFallbackExpression is not null)
                    EnsureState(declaration, state.HistoryFallbackExpression, state.HistoryFallbackStateKey,
                        state.HistoryLocation);
            }

            if (state.IsParallelComposite)
                EnsureParallelComposite(declaration, state.IdentityKey, state.ValueExpression, state.SourceLocation);
        }

        foreach (var membership in declaration.RegionMemberships)
        {
            var memberState = EnsureState(declaration, membership.StateExpression, membership.StateKey,
                membership.SourceLocation);
            memberState.ParentStateKey ??= membership.OwnerCompositeStateKey;
            memberState.ParentStateExpression ??= membership.OwnerCompositeExpression;
            memberState.ParentLocation ??= membership.SourceLocation;
            if (membership.IsTerminal) memberState.IsTerminal = true;

            var region = EnsureRegion(declaration, membership.OwnerCompositeStateKey,
                membership.OwnerCompositeExpression, membership.RegionName, false, membership.SourceLocation);
            region.AddMembership(membership);
            if (membership.IsInitial)
            {
                region.InitialStateKey = membership.StateKey;
                region.InitialStateExpression = membership.StateExpression;
                region.InitialLocation = membership.SourceLocation;
            }
        }

        foreach (var region in declaration.Regions.ToArray())
        {
            var canonical = EnsureRegion(declaration, region.OwnerCompositeStateKey, region.OwnerCompositeExpression,
                region.RegionName, region.IsExplicit, region.SourceLocation);
            if (!ReferenceEquals(canonical, region))
            {
                if (region.IsExplicit) canonical.MarkExplicit();
                canonical.AddRelatedLocation(region.SourceLocation);
            }
        }
    }

    private static DeclaredState EnsureState(MachineDeclaration declaration, string expression, string key,
        Microsoft.CodeAnalysis.Location? location)
    {
        return DeclarationParserHelpers.EnsureState(declaration, expression, key, location);
    }

    private static CompositeDeclaration MergeComposite(MachineDeclaration declaration, string key, string expression,
        Microsoft.CodeAnalysis.Location? location)
    {
        var existing = declaration.Composites.FirstOrDefault(c => c.CompositeStateKey == key);
        if (existing is not null) return existing;
        var composite = new CompositeDeclaration(key, expression, location);
        declaration.Composites.Add(composite);
        return composite;
    }

    private static void EnsureParallelComposite(MachineDeclaration declaration, string key, string expression,
        Microsoft.CodeAnalysis.Location? location)
    {
        var state = EnsureState(declaration, expression, key, location);
        state.IsParallelComposite = true;
        if (declaration.ParallelComposites.Any(p => p.CompositeStateKey == key)) return;
        declaration.ParallelComposites.Add(new ParallelCompositeDeclaration(key, expression, location));
    }

    private static RegionDeclaration EnsureRegion(MachineDeclaration declaration, string ownerKey, string ownerExpression,
        string name, bool isExplicit, Microsoft.CodeAnalysis.Location? location)
    {
        var existing = declaration.Regions.FirstOrDefault(r =>
            r.OwnerCompositeStateKey == ownerKey && string.Equals(r.RegionName, name, StringComparison.Ordinal));
        if (existing is not null) return existing;
        var region = new RegionDeclaration(ownerKey, ownerExpression, name, declaration.Regions.Count, isExplicit,
            location);
        declaration.Regions.Add(region);
        return region;
    }
}
