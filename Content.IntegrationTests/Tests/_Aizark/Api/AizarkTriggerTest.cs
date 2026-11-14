using Content.Shared._Aizark.Api.Components;
using Content.Shared.Trigger.Systems;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Aizark.Api;

/// <summary>
/// Проверяет работу AizarkApi вместе с настоящим TriggerSystem Dead Space.
/// Тест не зависит от модов и не изменяет набор игровых прототипов.
/// </summary>
[TestFixture]
public sealed class AizarkTriggerTest
{
    /// <summary>
    /// Счётчик одного предмета уменьшается до нуля, после чего условие
    /// запрещает дальнейшие срабатывания; другой предмет имеет независимое состояние.
    /// </summary>
    [Test]
    public async Task FiniteUseVariablesAndIsolation()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entities = server.ResolveDependency<IEntityManager>();
        var map = await pair.CreateTestMap();

        await server.WaitAssertion(() =>
        {
            var triggers = entities.System<TriggerSystem>();
            var first = entities.SpawnEntity(null, map.GridCoords);
            var second = entities.SpawnEntity(null, map.GridCoords);

            foreach (var uid in new[] { first, second })
            {
                var variables = entities.AddComponent<AizarkVariablesComponent>(uid);
                variables.Values["remaining"] = 3;

                var condition = entities.AddComponent<AizarkVariableConditionComponent>(uid);
                condition.Variable = "remaining";
                condition.Comparison = AizarkVariableComparison.Greater;
                condition.Value = 0;

                var effect = entities.AddComponent<AizarkVariableEffectComponent>(uid);
                effect.Variable = "remaining";
                effect.Operation = AizarkVariableOperation.Subtract;
                effect.Value = 1;
            }

            for (var count = 2; count >= 0; count--)
            {
                Assert.That(triggers.Trigger(first), Is.True);
                Assert.That(entities.GetComponent<AizarkVariablesComponent>(first).Values["remaining"],
                    Is.EqualTo(count));
            }

            Assert.That(triggers.Trigger(first), Is.False, "Использования должны закончиться.");
            Assert.That(entities.GetComponent<AizarkVariablesComponent>(second).Values["remaining"],
                Is.EqualTo(3), "Независимый предмет не должен расходовать общий запас.");

            entities.DeleteEntity(first);
            entities.DeleteEntity(second);
        });

        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// Условия AND и сравнение двух переменных проверяются до эффекта.
    /// Отсутствующий ключ никогда не считается равным нулю.
    /// </summary>
    [Test]
    public async Task CompositeConditionsAndMissingVariables()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entities = server.ResolveDependency<IEntityManager>();
        var map = await pair.CreateTestMap();

        await server.WaitAssertion(() =>
        {
            var triggers = entities.System<TriggerSystem>();
            var uid = entities.SpawnEntity(null, map.GridCoords);
            var variables = entities.AddComponent<AizarkVariablesComponent>(uid);
            variables.Values["stock"] = 2;
            variables.Values["minimum"] = 1;

            var rules = entities.AddComponent<AizarkRulesConditionComponent>(uid);
            rules.Join = AizarkRuleJoin.All;
            rules.Rules.Add(new AizarkVariableRule
            {
                Variable = "stock",
                Comparison = AizarkVariableComparison.Greater,
                OtherVariable = "minimum"
            });
            rules.Rules.Add(new AizarkVariableRule
            {
                Variable = "stock",
                Comparison = AizarkVariableComparison.LessOrEqual,
                Value = 2
            });

            var effect = entities.AddComponent<AizarkVariableEffectComponent>(uid);
            effect.Variable = "stock";
            effect.Operation = AizarkVariableOperation.Subtract;
            effect.Value = 1;

            Assert.That(triggers.Trigger(uid), Is.True);
            Assert.That(variables.Values["stock"], Is.EqualTo(1));
            Assert.That(triggers.Trigger(uid), Is.False, "stock больше не превышает minimum.");

            rules.Rules.Clear();
            rules.Rules.Add(new AizarkVariableRule
            {
                Variable = "missing",
                Comparison = AizarkVariableComparison.Equal,
                Value = 0
            });
            Assert.That(triggers.Trigger(uid), Is.False, "Отсутствующий ключ — не ноль.");

            entities.DeleteEntity(uid);
        });

        await pair.CleanReturnAsync();
    }
}
