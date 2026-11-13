using Content.Shared._Aizark.Api.Components;
using Content.Shared.Trigger.Components;
using Robust.Shared.Network;

namespace Content.Shared._Aizark.Api.Systems;

/// <summary>
/// Расширение штатной системы триггеров SS14 для проверки и изменения
/// целочисленных переменных отдельных сущностей.
/// </summary>
/// <remarks>
/// Система регистрирует собственные подписки на AttemptTriggerEvent и TriggerEvent.
/// Правок в Content.Shared.Trigger.Systems.TriggerSystem или RobustToolbox нет.
/// Условия читаются обеими сторонами, а изменение переменных выполняет сервер.
/// </remarks>
public sealed class AizarkVariableSystem : EntitySystem
{
    /// <summary>Определяет авторитетную сторону для изменения состояния.</summary>
    [Dependency] private readonly INetManager _net = default!;

    /// <summary>Подключает проверку до триггера и эффект после его успешного запуска.</summary>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AizarkVariableConditionComponent, AttemptTriggerEvent>(OnAttempt);
        SubscribeLocalEvent<AizarkVariableEffectComponent, TriggerEvent>(OnTriggered);
    }

    /// <summary>
    /// Обрабатывает условие сравнения. При любой ошибке разрешения цели или
    /// отсутствующем ключе запрещает триггер (fail closed).
    /// </summary>
    private void OnAttempt(Entity<AizarkVariableConditionComponent> ent, ref AttemptTriggerEvent args)
    {
        var config = ent.Comp;
        if (args.Key != null && args.Key != config.Key)
            return;

        // По умолчанию переменные локальны для предмета; TargetUser
        // переключает поиск на сущность пользователя, активировавшего предмет.
        var target = config.TargetUser ? args.User : ent.Owner;
        if (target == null ||
            !TryComp<AizarkVariablesComponent>(target.Value, out var variables) ||
            !variables.Values.TryGetValue(config.Variable, out var current))
        {
            args.Cancelled = true;
            return;
        }

        // Не заменяем уже выставленный Cancelled=false/true других условий;
        // добавляем свой результат к общей проверке.
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

    /// <summary>
    /// Выполняет изменение переменной после успешного прохождения условий.
    /// Игнорирует отсутствующие ключи, некорректное деление и переполнение.
    /// </summary>
    private void OnTriggered(Entity<AizarkVariableEffectComponent> ent, ref TriggerEvent args)
    {
        // Даже если клиент вызвал предсказанный TriggerEvent, переменные
        // нельзя расходовать дважды или изменять из недоверенной стороны.
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
            // Важнее сохранить корректное состояние, чем записать переполненный int.
            return;
        }

        if (next == old)
            return;

        variables.Values[config.Variable] = next;
        Dirty(target.Value, variables); // Синхронизируем изменённый компонент.
        args.Handled = true;
    }
}
