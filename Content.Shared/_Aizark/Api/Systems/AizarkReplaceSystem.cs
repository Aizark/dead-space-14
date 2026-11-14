using Content.Shared._Aizark.Api.Components;
using Content.Shared.Trigger;
using Robust.Shared.Network;

namespace Content.Shared._Aizark.Api.Systems;

/// <summary>
/// Серверное действие замены сущности, запускаемое штатным TriggerEvent.
/// </summary>
/// <remarks>
/// Не используйте вместе с AizarkDispenseOnTrigger для обработки одного и того же
/// последнего использования: у диспенсера уже есть собственная замена пустой оболочки.
/// Выполнение других эффектов не должно зависеть от порядка подписок на TriggerEvent.
/// </remarks>
public sealed class AizarkReplaceSystem : EntitySystem
{
    /// <summary>Сторона, имеющая право изменять состав мира.</summary>
    [Dependency] private readonly INetManager _net = default!;

    /// <summary>Регистрирует действие при успешном срабатывании штатного триггера.</summary>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AizarkReplaceOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    /// <summary>
    /// Создаёт заменяющий прототип в координатах владельца, после чего
    /// планирует удаление исходного предмета. Пустой ключ триггера от
    /// системного вызова считается широковещательным, как в оригинальном SS14.
    /// </summary>
    private void OnTrigger(Entity<AizarkReplaceOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (!_net.IsServer || (args.Key != null && args.Key != ent.Comp.Key))
            return;

        var location = Transform(ent).Coordinates;
        if (!location.IsValid(EntityManager))
            return;

        Spawn(ent.Comp.Prototype, location);
        QueueDel(ent.Owner);
        args.Handled = true;
    }
}
