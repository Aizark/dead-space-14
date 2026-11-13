using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Aizark.Api.Components;

/// <summary>
/// Server-authoritative, finite-use dispenser. Gives one randomly selected item to a hand
/// when possible, otherwise drops it near the user. Optionally replaces itself on depletion.
/// Reusable for anything from consumable boxes to vending-like gadgets.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AizarkDispenseOnTriggerComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Key = "trigger";

    [DataField(required: true)]
    public EntityTableSelector Table = default!;

    /// <summary>Key of an existing integer in AizarkVariables on this entity.</summary>
    [DataField, AutoNetworkedField]
    public string StockVariable = "remaining";

    [DataField, AutoNetworkedField]
    public EntProtoId? EmptyPrototype;
}
