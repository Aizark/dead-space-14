using System.Linq;
using Content.Shared._Aizark.Api.Components;
using Content.Shared.EntityTable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Trigger.Components;
using Robust.Shared.Network;

namespace Content.Shared._Aizark.Api.Systems;

/// <summary>
/// Общая система выдачи одного случайного предмета из конечного запаса.
/// Работает поверх оригинального TriggerEvent без модификации TriggerSystem.
/// </summary>
/// <remarks>
/// Все изменения запаса и создание сущностей выполняются только на сервере.
/// Если пользователь имеет свободную подходящую руку, предмет попадает туда;
/// иначе остаётся у ног. При исчерпании диспенсер заменяется пустым прототипом.
/// Эта система универсальна и не ссылается ни на один конкретный мод или предмет.
/// </remarks>
public sealed class AizarkDispenseSystem : EntitySystem
{
    /// <summary>Запрещает клиенту создавать предметы и изменять запас.</summary>
    [Dependency] private readonly INetManager _net = default!;
    /// <summary>Выбирает прототипы по штатным правилам таблиц сущностей.</summary>
    [Dependency] private readonly EntityTableSystem _tables = default!;
    /// <summary>Использует обычные проверки захвата предмета в свободную руку.</summary>
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    /// <summary>Подключает эффект выдачи к штатному событию срабатывания.</summary>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AizarkDispenseOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    /// <summary>
    /// Проверяет ключ и остаток; выбирает предмет, создаёт его, уменьшает запас
    /// и, если он закончился, заменяет диспенсер на заданный прототип.
    /// </summary>
    private void OnTrigger(Entity<AizarkDispenseOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (!_net.IsServer || (args.Key != null && args.Key != ent.Comp.Key))
            return;

        // Переменная обязана существовать на самом диспенсере. При нуле
        // или отсутствии значения ничего не создаём и не расходуем.
        if (!TryComp<AizarkVariablesComponent>(ent, out var variables) ||
            !variables.Values.TryGetValue(ent.Comp.StockVariable, out var stock) ||
            stock <= 0)
            return;

        // Таблица должна быть настроена на ровно один случайный результат.
        // Используем штатную EntityTableSystem, а не собственный RNG.
        var selected = _tables.GetSpawns(ent.Comp.Table).FirstOrDefault();
        if (string.IsNullOrEmpty(selected.Id))
            return;

        // Предмет возникает рядом с инициатором либо у самого источника.
        // Затем штатная система рук пробует его подобрать. Если не удалось,
        // созданная сущность остаётся в мире, не отнимая занятую руку.
        var recipient = args.User;
        var spawnLocation = recipient != null ? Transform(recipient.Value).Coordinates : Transform(ent).Coordinates;
        if (!spawnLocation.IsValid(EntityManager))
            return;

        var spawned = Spawn(selected, spawnLocation);
        if (recipient != null)
            _hands.TryPickupAnyHand(recipient.Value, spawned, animate: false);

        // Сначала успешное создание, затем расходование одной единицы запаса.
        variables.Values[ent.Comp.StockVariable] = stock - 1;
        Dirty(ent.Owner, variables);
        args.Handled = true;

        if (stock != 1 || ent.Comp.EmptyPrototype == null)
            return;

        // Важно: это отдельная оболочка, а не Storage с нулём содержимого.
        // Удаление исходной сущности освобождает и её собственную руку, если
        // игрок продолжал держать упаковку при последней выдаче.
        var oldLocation = Transform(ent).Coordinates;
        if (oldLocation.IsValid(EntityManager))
            Spawn(ent.Comp.EmptyPrototype.Value, oldLocation);
        QueueDel(ent.Owner);
    }
}
