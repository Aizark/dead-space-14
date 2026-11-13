using Content.Shared._Aizark.Api.Components;
using Content.Shared.Trigger;
using Robust.Shared.Network;

namespace Content.Shared._Aizark.Api.Systems;

/// <summary>
/// Adds stateful comparisons and integer operations to the existing SS14 trigger event pipeline.
/// Does not patch TriggerSystem or RobustToolbox.
/// </summary>
public sealed class AizarkVariableSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AizarkVariableConditionComponent, AttemptTriggerEvent>(OnAttempt);
        SubscribeLocalEvent<AizarkVariableEffectComponent, TriggerEvent>(OnTriggered);
    }

    private void OnAttempt(Entity<AizarkVariableConditionComponent> ent, ref AttemptTriggerEvent args)
    {
        var config = ent.Comp;
        if (args.Key != null && args.Key != config.Key)
            return;

        var target = config.TargetUser ? args.User : ent.Owner;
        if (target == null ||
            !TryComp<AizarkVariablesComponent>(target.Value, out var variables) ||
            !variables.Values.TryGetValue(config.Variable, out var current))
        {
            args.Cancelled = true;
            return;
        }

        var valid = config.Comparison switch
        {
            AizarkVariableComparison.Equal => current == config.Value,
            AizarkVariableComparison.NotEqual => current != config.Value,
            AizarkVariableComparison.Greater => current > config.Value,
            AizarkVariableComparison.GreaterOrEqual => current >= config.Value,
            AizarkVariableComparison.Less => current < config.Value,
            AizarkVariableComparison.LessOrEqual => current <= config.Value,
            _ => false
        };
        args.Cancelled |= !valid;
    }

    private void OnTriggered(Entity<AizarkVariableEffectComponent> ent, ref TriggerEvent args)
    {
        if (!_net.IsServer)
            return;

        var config = ent.Comp;
        if (args.Key != null && args.Key != config.Key)
            return;

        var target = config.TargetUser ? args.User : ent.Owner;
        if (target == null || !TryComp<AizarkVariablesComponent>(target.Value, out var variables))
            return;

        if (!variables.Values.TryGetValue(config.Variable, out var old))
            return;

        int next;
        try
        {
            next = config.Operation switch
            {
                AizarkVariableOperation.Set => config.Value,
                AizarkVariableOperation.Add => checked(old + config.Value),
                AizarkVariableOperation.Subtract => checked(old - config.Value),
                AizarkVariableOperation.Multiply => checked(old * config.Value),
                AizarkVariableOperation.Divide when config.Value != 0 => old / config.Value,
                AizarkVariableOperation.Min => Math.Min(old, config.Value),
                AizarkVariableOperation.Max => Math.Max(old, config.Value),
                _ => old
            };
        }
        catch (OverflowException)
        {
            return;
        }

        if (next == old)
            return;

        variables.Values[config.Variable] = next;
        Dirty(target.Value, variables);
        args.Handled = true;
    }
}
