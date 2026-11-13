using Robust.Shared.GameStates;

namespace Content.Shared._Aizark.Api.Components;

public enum AizarkVariableComparison : byte
{
    Equal,
    NotEqual,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AizarkVariableConditionComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Key = "trigger";

    [DataField(required: true), AutoNetworkedField]
    public string Variable = string.Empty;

    [DataField, AutoNetworkedField]
    public AizarkVariableComparison Comparison = AizarkVariableComparison.Greater;

    [DataField, AutoNetworkedField]
    public int Value;

    [DataField, AutoNetworkedField]
    public bool TargetUser;

    // Missing variables fail closed, rather than silently defaulting to zero.
}
