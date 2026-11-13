using System.Linq;
using Content.Shared._Aizark.Api.Components;
using Content.Shared.EntityTable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Trigger.Components;
using Robust.Shared.Network;

namespace Content.Shared._Aizark.Api.Systems;

/// <summary>
/// Atomic, server-only trigger dispenser. Does not require changes to the native trigger system.
/// </summary>
public sealed class AizarkDispenseSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly EntityTableSystem _tables = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AizarkDispenseOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<AizarkDispenseOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (!_net.IsServer || (args.Key != null && args.Key != ent.Comp.Key))
            return;

        if (!TryComp<AizarkVariablesComponent>(ent, out var variables) ||
            !variables.Values.TryGetValue(ent.Comp.StockVariable, out var stock) ||
            stock <= 0)
            return;

        var selected = _tables.GetSpawns(ent.Comp.Table).FirstOrDefault();
        if (string.IsNullOrEmpty(selected.Id))
            return;

        var recipient = args.User;
        var spawnLocation = recipient != null ? Transform(recipient.Value).Coordinates : Transform(ent).Coordinates;
        if (!spawnLocation.IsValid(EntityManager))
            return;

        var spawned = Spawn(selected, spawnLocation);
        if (recipient != null)
            _hands.TryPickupAnyHand(recipient.Value, spawned, animate: false);

        // Spawn must succeed before stock changes. Only the server mutates it.
        variables.Values[ent.Comp.StockVariable] = stock - 1;
        Dirty(ent.Owner, variables);
        args.Handled = true;

        if (stock != 1 || ent.Comp.EmptyPrototype == null)
            return;

        // Keep the exhausted shell in-world, instead of deleting the final container.
        // Spawn at the source entity's current coordinates, then remove the original.
        var oldLocation = Transform(ent).Coordinates;
        if (oldLocation.IsValid(EntityManager))
            Spawn(ent.Comp.EmptyPrototype.Value, oldLocation);
        QueueDel(ent.Owner);
    }
}
