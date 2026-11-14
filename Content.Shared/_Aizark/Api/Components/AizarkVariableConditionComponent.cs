using Robust.Shared.GameStates;

namespace Content.Shared._Aizark.Api.Components;

/// <summary>
/// Поддерживаемые сравнения числовой переменной с константой.
/// </summary>
public enum AizarkVariableComparison : byte
{
    /// <summary>Значение равно порогу.</summary>
    Equal,
    /// <summary>Значение не равно порогу.</summary>
    NotEqual,
    /// <summary>Значение строго больше порога.</summary>
    Greater,
    /// <summary>Значение больше либо равно порогу.</summary>
    GreaterOrEqual,
    /// <summary>Значение строго меньше порога.</summary>
    Less,
    /// <summary>Значение меньше либо равно порогу.</summary>
    LessOrEqual
}

/// <summary>
/// Проверка переменной до исполнения штатного триггера SS14.
/// Подписывается на AttemptTriggerEvent, отменяя срабатывание при ложном условии.
/// </summary>
/// <remarks>
/// Не заменяет TriggerSystem и не изменяет существующие компоненты.
/// Проверяет переменные на своей сущности либо на пользователе, инициировавшем триггер.
/// Если цель, компонент хранилища или ключ отсутствуют, условие не проходит.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AizarkVariableConditionComponent : Component
{
    /// <summary>
    /// Ключ триггера, который проверяется. Значение по умолчанию — trigger.
    /// Должен совпадать с ключом вызывающего триггера.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string Key = "trigger";

    /// <summary>Имя переменной из AizarkVariables.Values, например remaining.</summary>
    [DataField(required: true), AutoNetworkedField]
    public string Variable = string.Empty;

    /// <summary>Операция сравнения; по умолчанию строго больше.</summary>
    [DataField, AutoNetworkedField]
    public AizarkVariableComparison Comparison = AizarkVariableComparison.Greater;

    /// <summary>Постоянный порог, с которым сравнивается переменная.</summary>
    [DataField, AutoNetworkedField]
    public int Value;

    /// <summary>
    /// false: проверять владельца компонента. true: проверить пользователя
    /// из события триггера. Если пользователь отсутствует, условие не выполнится.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool TargetUser;
}
