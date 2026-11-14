using Content.Shared._Aizark.Api.Components;
using Content.Shared.Trigger.Components;

namespace Content.Shared._Aizark.Api.Systems;

/// <summary>
/// Выполняет составные правила переменных поверх стандартного AttemptTriggerEvent.
/// Учитывает AND, OR, NOT и сравнение двух переменных на разных сущностях.
/// </summary>
public sealed class AizarkRulesSystem : EntitySystem
{
    /// <summary>Подписывает систему на фазу проверки штатного триггера.</summary>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AizarkRulesConditionComponent, AttemptTriggerEvent>(OnAttempt);
    }

    /// <summary>
    /// Вычисляет правила и при неуспехе выставляет Cancelled.
    /// Чужие результаты условий не перезаписывает.
    /// </summary>
    private void OnAttempt(Entity<AizarkRulesConditionComponent> ent, ref AttemptTriggerEvent args)
    {
        var comp = ent.Comp;
        if (args.Key != null && args.Key != comp.Key)
            return;

        // Пустая группа никогда не может разрешить триггер.
        // При инверсии оставляем это поведение (не делаем пустое NOT истинным).
        if (comp.Rules.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        var passed = comp.Join == AizarkRuleJoin.All;
        foreach (var rule in comp.Rules)
        {
            var ok = Evaluate(ent.Owner, args.User, rule);
            if (comp.Join == AizarkRuleJoin.All)
            {
                if (ok)
                    continue;
                passed = false;
                break;
            }

            if (!ok)
                continue;
            passed = true;
            break;
        }

        args.Cancelled |= !(comp.Not ? !passed : passed);
    }

    /// <summary>
    /// Сравнивает заданные значения; отсутствие сущности или ключа означает false.
    /// Для двух переменных правая часть разрешается независимо от левой.
    /// </summary>
    private bool Evaluate(EntityUid owner, EntityUid? user, AizarkVariableRule rule)
    {
        var lhsTarget = rule.TargetUser ? user : owner;
        if (!Read(lhsTarget, rule.Variable, out var lhs))
            return false;

        var rhs = rule.Value;
        if (rule.OtherVariable != null)
        {
            var rhsTarget = rule.OtherTargetUser ? user : owner;
            if (!Read(rhsTarget, rule.OtherVariable, out rhs))
                return false;
        }

        var matched = rule.Comparison switch
        {
            AizarkVariableComparison.Equal => lhs == rhs,
            AizarkVariableComparison.NotEqual => lhs != rhs,
            AizarkVariableComparison.Greater => lhs > rhs,
            AizarkVariableComparison.GreaterOrEqual => lhs >= rhs,
            AizarkVariableComparison.Less => lhs < rhs,
            AizarkVariableComparison.LessOrEqual => lhs <= rhs,
            _ => false
        };
        return rule.Not ? !matched : matched;
    }

    /// <summary>
    /// Безопасное получение переменной с конкретной сущности, без создания
    /// нового компонента, новых ключей или неявного значения по умолчанию.
    /// </summary>
    private bool Read(EntityUid? target, string key, out int value)
    {
        value = default;
        return target != null &&
               TryComp<AizarkVariablesComponent>(target.Value, out var variables) &&
               variables.Values.TryGetValue(key, out value);
    }
}
