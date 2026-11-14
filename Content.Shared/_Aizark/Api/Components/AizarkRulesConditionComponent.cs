using Content.Shared._Aizark.Api.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Aizark.Api.Components;

/// <summary>
/// Способ объединения нескольких проверок в одно логическое условие.
/// </summary>
public enum AizarkRuleJoin : byte
{
    /// <summary>Все правила должны пройти (логическое И).</summary>
    All,
    /// <summary>Достаточно одного прошедшего правила (логическое ИЛИ).</summary>
    Any
}

/// <summary>
/// Отдельное сравнение в последовательности правил.
/// Левая переменная ищется на владельце триггера или его пользователе.
/// Правая часть может быть константой или другой переменной.
/// </summary>
[DataDefinition]
public sealed partial class AizarkVariableRule
{
    /// <summary>Имя левой переменной.</summary>
    [DataField(required: true)]
    public string Variable = string.Empty;

    /// <summary>Если true, левая переменная принадлежит пользователю.</summary>
    [DataField]
    public bool TargetUser;

    /// <summary>Оператор сравнения.</summary>
    [DataField]
    public AizarkVariableComparison Comparison = AizarkVariableComparison.Greater;

    /// <summary>Числовая константа, если OtherVariable не задана.</summary>
    [DataField]
    public int Value;

    /// <summary>
    /// Необязательная правая переменная: позволяет сравнить два значения.
    /// При отсутствии используется Value. Пустая строка означает константу.
    /// </summary>
    [DataField]
    public string? OtherVariable;

    /// <summary>
    /// Если OtherVariable задана, true ищет её у пользователя,
    /// false — у сущности, содержащей условие.
    /// </summary>
    [DataField]
    public bool OtherTargetUser;

    /// <summary>Инвертировать результат отдельного сравнения (логическое НЕ).</summary>
    [DataField]
    public bool Not;
}

/// <summary>
/// Составное условие на AttemptTriggerEvent.
/// Содержит несколько правил и объединяет их через All/Any.
/// </summary>
/// <remarks>
/// Этот компонент не меняет исходную систему триггеров SS14.
/// Проверки используют значения только конкретных сущностей.
/// При отсутствии переменной или цели правило не проходит.
/// Список без правил считается недействительным.
/// </remarks>
[RegisterComponent]
public sealed partial class AizarkRulesConditionComponent : Component
{
    /// <summary>Ключ существующего триггера SS14.</summary>
    [DataField]
    public string Key = "trigger";

    /// <summary>Логическое объединение проверок.</summary>
    [DataField]
    public AizarkRuleJoin Join = AizarkRuleJoin.All;

    /// <summary>Список отдельных проверок. YAML задаёт в порядке для чтения.</summary>
    [DataField]
    public List<AizarkVariableRule> Rules = new();

    /// <summary>Инвертировать итоговую группу правил (НЕ от AND/OR).</summary>
    [DataField]
    public bool Not;
}
