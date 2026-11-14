using System.Linq;
using Content.Shared._Aizark.Api.Components;
using Content.Shared.EntityTable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Trigger.Components;
using Robust.Shared.Network;

namespace Content.Shared._Aizark.Api.Systems;

/// <summary>
/// Реализует переиспользуемый эффект выдачи предмета без ограничения запаса.
/// </summary>
/// <remarks>
/// Система намеренно отделена от ограниченного диспенсера: одна сущность может
/// выдавать предметы по условиям других компонентов TriggerSystem, не имея счётчика.
/// Не запрашивает предсказанный спавн: случайные EntityTable не гарантируют одинаковый
/// результат при независимом клиентском и серверном вычислении.
/// </remarks>
public sealed class AizarkSpawnToHandsSystem : EntitySystem
{
    /// <summary>Авторитетная сторона создания сущностей.</summary>
    [Dependency] private readonly INetManager _net = default!;
    /// <summary>Штатный механизм выбора прототипов.</summary>
    [Dependency] private readonly EntityTableSystem _tables = default!;
    /// <summary>Штатное взаимодействие с инвентарём рук.</summary>
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    /// <summary>Регистрирует обработчик успешного TriggerEvent.</summary>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AizarkSpawnToHandsOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    /// <summary>
    /// Создаёт предмет у пользователя, после чего пытается положить его в руку.
    /// Если руки нет или подбор запрещён, предмет остаётся на полу.
    /// </summary>
    private void OnTrigger(Entity<AizarkSpawnToHandsOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (!_net.IsServer || (args.Key != null && args.Key != ent.Comp.Key))
            return;

        var selected = _tables.GetSpawns(ent.Comp.Table).FirstOrDefault();
        if (string.IsNullOrEmpty(selected.Id))
            return;

        var recipient = args.User;
        var location = recipient != null ? Transform(recipient.Value).Coordinates : Transform(ent).Coordinates;
        if (!location.IsValid(EntityManager))
            return;

        var created = Spawn(selected, location);
        if (recipient != null)
            _hands.TryPickupAnyHand(recipient.Value, created, animate: false);

        args.Handled = true;
    }
}
