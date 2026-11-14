using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Aizark.Api.Components;

/// <summary>
/// Универсальный ограниченный диспенсер для штатного TriggerEvent.
/// Выбирает предмет из таблицы прототипов, пытается передать его в свободную
/// руку пользователя и при невозможности оставляет у ног.
/// </summary>
/// <remarks>
/// Не содержит знаний о конфетах или других конкретных предметах.
/// Запас считывается из AizarkVariables той же сущности и уменьшается
/// только после создания выбранного предмета. При последней выдаче может
/// создать альтернативный прототип пустой оболочки.
/// Выполняет спавн только на сервере: таблица случайного выбора не предсказывается.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AizarkDispenseOnTriggerComponent : Component
{
    /// <summary>Ключ штатного триггера, запускающего выдачу.</summary>
    [DataField, AutoNetworkedField]
    public string Key = "trigger";

    /// <summary>
    /// Таблица выбора создаваемых прототипов. Настраивается через YAML и
    /// должна выдавать ровно один прототип за активацию; система берёт первый результат.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector Table = default!;

    /// <summary>
    /// Название уже существующего целочисленного счётчика в AizarkVariables
    /// на владельце. Выдача возможна только пока значение строго положительно.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string StockVariable = "remaining";

    /// <summary>
    /// Необязательный прототип, заменяющий израсходованный предмет.
    /// Например, пустая коробка. null оставляет исходную сущность без запаса.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId? EmptyPrototype;
}
