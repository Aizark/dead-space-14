using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.GameStates;

namespace Content.Shared._Aizark.Api.Components;

/// <summary>
/// Универсальное действие создания случайного предмета в свободной руке.
/// Если свободной подходящей руки нет, предмет остаётся у ног пользователя.
/// </summary>
/// <remarks>
/// Использует штатную таблицу сущностей и TriggerEvent.
/// Не требует AizarkVariables: подходит для бесконечных источников,
/// наград за события и других модов, которым не нужен счётчик запаса.
/// Выбор случайной сущности выполняется только сервером.
/// </remarks>
[RegisterComponent]
public sealed partial class AizarkSpawnToHandsOnTriggerComponent : Component
{
    /// <summary>Ключ триггера SS14, который должен вызвать действие.</summary>
    [DataField]
    public string Key = "trigger";

    /// <summary>
    /// Таблица выбора предмета. Настраивается так, чтобы давать ровно один
    /// результат. Если таблица пуста, действие ничего не создаёт.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector Table = default!;
}
