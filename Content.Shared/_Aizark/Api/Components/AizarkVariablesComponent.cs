using Robust.Shared.GameStates;

namespace Content.Shared._Aizark.Api.Components;

/// <summary>
/// Per-entity, namespaced integer variables for Aizark content.
/// Never use these for security or authorization decisions.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AizarkVariablesComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<string, int> Values = new();
}
