using Robust.Shared.GameStates;

namespace Content.Shared._Aizark.Api.Components;

public enum AizarkVariableOperation : byte
{
    Set,
    Add,
    Subtract,
    Multiply,
    Divide,
    Min,
    Max
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AizarkVariableEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Key = "trigger";

    [DataField(required: true), AutoNetworkedField]
    public string Variable = string.Empty;

    [DataField, AutoNetworkedField]
    public AizarkVariableOperation Operation = AizarkVariableOperation.Subtract;

    [DataField, AutoNetworkedField]
    public int Value = 1;

    [DataField, AutoNetworkedField]
    public bool TargetUser;
}
