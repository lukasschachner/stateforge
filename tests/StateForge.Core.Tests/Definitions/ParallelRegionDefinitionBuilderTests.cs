using StateForge.Core.Definitions;
using StateForge.Core.Tests.Parallel;
using StateForge.Core.Validation;

namespace StateForge.Core.Tests.Definitions;

public sealed class ParallelRegionDefinitionBuilderTests
{
    [Fact]
    public void Builder_declares_parallel_composite_regions_and_membership()
    {
        var definition = ParallelGraphTestData.CreateTwoRegionDefinition();

        Assert.True(definition.IsParallelComposite(ParallelState.Operational));
        Assert.Equal(new[] { "Fulfillment", "Billing" },
            definition.GetParallelRegions(ParallelState.Operational).Select(r => r.Name));
        Assert.True(definition.TryGetRegionMembership(ParallelState.WaitingForPick, out var membership));
        Assert.Equal("Fulfillment",
            definition.GetParallelRegions(ParallelState.Operational).Single(r => r.RegionId == membership.RegionId)
                .Name);
    }

    [Fact]
    public void Nested_region_block_syntax_declares_equivalent_regions()
    {
        var definition = ParallelGraphTestData.CreateTwoRegionDefinitionNewStyle();

        Assert.Empty(definition.Validate().Errors);
        Assert.True(definition.IsParallelComposite(ParallelState.Operational));
        Assert.Equal(["Fulfillment", "Billing"],
            definition.GetParallelRegions(ParallelState.Operational).Select(r => r.Name));
        AssertRegion(definition, ParallelState.WaitingForPick, "Fulfillment");
        AssertRegion(definition, ParallelState.Packing, "Fulfillment");
        AssertRegion(definition, ParallelState.FulfillmentDone, "Fulfillment");
        AssertRegion(definition, ParallelState.WaitingForPayment, "Billing");
        AssertRegion(definition, ParallelState.CapturingPayment, "Billing");
        AssertRegion(definition, ParallelState.BillingDone, "Billing");
    }

    [Fact]
    public void Region_scoped_state_declarations_use_existing_transition_semantics()
    {
        var definition = ParallelGraphTestData.CreateTwoRegionDefinitionNewStyle();
        var transitions = definition.Transitions.Select(t => (t.SourceState, t.TargetState, Event: t.Event.DisplayName)).ToArray();

        Assert.Contains((ParallelState.WaitingForPick, ParallelState.Packing, nameof(ParallelEvent.PickStarted)), transitions);
        Assert.Contains((ParallelState.Packing, ParallelState.FulfillmentDone,
                nameof(ParallelEvent.CompleteFulfillment)),
            transitions);
        Assert.Contains((ParallelState.WaitingForPayment, ParallelState.CapturingPayment,
                nameof(ParallelEvent.PaymentStarted)),
            transitions);
        Assert.Contains((ParallelState.CapturingPayment, ParallelState.BillingDone, nameof(ParallelEvent.CompleteBilling)),
            transitions);
    }

    [Fact]
    public void Region_scoped_initial_assigns_membership_and_initial_state()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational,
                composite => composite.Region("Fulfillment", region => region.Initial(ParallelState.WaitingForPick)));
        });

        var region = Assert.Single(definition.GetParallelRegions(ParallelState.Operational));
        Assert.True(region.HasInitialState);
        Assert.Equal(ParallelState.WaitingForPick, region.InitialState);
        Assert.Contains(ParallelState.WaitingForPick, region.MemberStates);
        AssertRegion(definition, ParallelState.WaitingForPick, "Fulfillment");
    }

    [Fact]
    public void Region_scoped_terminal_assigns_membership_terminal_role_and_state_marker()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational,
                composite => composite.Region("Fulfillment", region => region.Terminal(ParallelState.FulfillmentDone)));
        });

        var region = Assert.Single(definition.GetParallelRegions(ParallelState.Operational));
        Assert.Contains(ParallelState.FulfillmentDone, region.MemberStates);
        Assert.Contains(ParallelState.FulfillmentDone, region.TerminalStates);
        Assert.True(definition.FindState(ParallelState.FulfillmentDone)!.IsTerminal);
        AssertRegion(definition, ParallelState.FulfillmentDone, "Fulfillment");
    }

    [Fact]
    public void Repeated_same_region_declarations_are_idempotent_for_members_and_terminals()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational, composite => composite.Region("Fulfillment", region =>
            {
                region.Initial(ParallelState.WaitingForPick);
                region.State(ParallelState.WaitingForPick);
                region.Terminal(ParallelState.FulfillmentDone);
                region.Terminal(ParallelState.FulfillmentDone);
            }));
        });

        var region = Assert.Single(definition.GetParallelRegions(ParallelState.Operational));
        Assert.Equal([ParallelState.WaitingForPick, ParallelState.FulfillmentDone], region.MemberStates);
        Assert.Equal([ParallelState.FulfillmentDone], region.TerminalStates);
    }

    [Fact]
    public void Existing_region_apis_remain_source_compatible()
    {
        var definition = ParallelGraphTestData.CreateTwoRegionDefinitionOldStyle();

        Assert.Empty(definition.Validate().Errors);
        Assert.Equal(["Fulfillment", "Billing"],
            definition.GetParallelRegions(ParallelState.Operational).Select(r => r.Name));
    }

    [Fact]
    public void Mixed_old_and_new_declarations_are_valid_when_membership_agrees()
    {
        var definition = ParallelGraphTestData.CreateTwoRegionDefinitionMixedStyle();

        Assert.Empty(definition.Validate().Errors);
        AssertRegion(definition, ParallelState.WaitingForPick, "Fulfillment");
        AssertRegion(definition, ParallelState.WaitingForPayment, "Billing");
    }

    [Fact]
    public void Mixed_declarations_preserve_region_and_member_declaration_order()
    {
        var definition = ParallelGraphTestData.CreateTwoRegionDefinitionMixedStyle();
        var regions = definition.GetParallelRegions(ParallelState.Operational);

        Assert.Equal(["Fulfillment", "Billing"], regions.Select(r => r.Name));
        Assert.Equal([ParallelState.WaitingForPick, ParallelState.Packing, ParallelState.FulfillmentDone],
            regions[0].MemberStates);
    }

    [Fact]
    public void Region_builder_exposes_context_and_records_metadata()
    {
        string? regionId = null;
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational, composite => composite.Region("Fulfillment", region =>
            {
                Assert.Equal(ParallelState.Operational, region.CompositeState);
                Assert.Equal("Fulfillment", region.RegionName);
                Assert.False(string.IsNullOrWhiteSpace(region.RegionId));
                regionId = region.RegionId;
                region.WithMetadata("doc", "value");
            }));
        });

        var region = Assert.Single(definition.GetParallelRegions(ParallelState.Operational));
        Assert.Equal(regionId, region.RegionId);
        Assert.Equal("value", region.Metadata["doc"]);
    }

    [Fact]
    public void Top_level_parallel_region_block_marks_composite_and_configures_region()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelRegion(ParallelState.Operational, "Fulfillment",
                region => region.Initial(ParallelState.WaitingForPick));
        });

        Assert.True(definition.IsParallelComposite(ParallelState.Operational));
        AssertRegion(definition, ParallelState.WaitingForPick, "Fulfillment");
    }

    [Fact]
    public void Region_ids_are_declaration_based_not_state_string_based()
    {
        var ownerA = new DuplicateLabelState("same");
        var ownerB = new DuplicateLabelState("same");
        var initialA = new DuplicateLabelState("initial");
        var initialB = new DuplicateLabelState("initial");

        var definition = StateMachineDefinition<DuplicateLabelState, string>.Create(builder =>
        {
            builder.ParallelComposite(ownerA).Region("Region", initialA, initialA);
            builder.ParallelComposite(ownerB).Region("Region", initialB, initialB);
        });

        var regionIds = definition.ParallelRegions.Select(region => region.RegionId).ToArray();

        Assert.Equal(["region-000", "region-001"], regionIds);
        Assert.Equal(regionIds.Length, regionIds.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void New_block_overloads_reject_null_callbacks()
    {
        var builder = new StateMachineDefinitionBuilder<ParallelState, ParallelEvent>();
        var composite = builder.ParallelComposite(ParallelState.Operational);

        Assert.Throws<ArgumentNullException>(() => composite.Region("Fulfillment", null!));
        Assert.Throws<ArgumentNullException>(() => builder.ParallelRegion(ParallelState.Operational, "Fulfillment", null!));
    }

    private sealed class DuplicateLabelState(string label)
    {
        public override string ToString() => label;
    }

    private static void AssertRegion(StateMachineDefinition<ParallelState, ParallelEvent> definition,
        ParallelState state, string expectedRegionName)
    {
        Assert.True(definition.TryGetRegionMembership(state, out var membership));
        var region = Assert.Single(definition.GetParallelRegions(ParallelState.Operational),
            r => r.RegionId == membership.RegionId);
        Assert.Equal(expectedRegionName, region.Name);
    }
}
