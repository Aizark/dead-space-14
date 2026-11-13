using Robust.Shared.GameStates;

namespace Content.Shared._Aizark.Api.Components;

/// <summary>Арифметические операции над целочисленными переменными.</summary>
public enum AizarkVariableOperation : byte
{
    /// <summary>Установить значение равным константе.</summary>
    Set,
    /// <summary>Прибавить константу.</summary>
    Add,
    /// <summary>Вычесть константу.</summary>
    Subtract,
    /// <summary>Умножить на константу.</summary>
    Multiply,
    /// <summary>Целочисленное деление на ненулевую константу.</summary>
    Divide,
    /// <summary>Выбрать меньшее из текущего значения и константы.</summary>
    Min,
    /// <summary>Выбрать большее из текущего значения и константы.</summary>
    Max
}

/// <summary>
/// Эффект штатного TriggerEvent: изменяет переменную сущности на сервере.
/// </summary>
/// <remarks>
/// В отличие от условия AizarkVariableCondition, этот компонент выполняет запись.
/// Не используйте одновременно с AizarkDispenseOnTrigger для уменьшения одного
/// счётчика: диспенсер самостоятельно расходует запас только после выдачи предмета.
/// Операции, приводящие к переполнению int, игнорируются; деление на ноль запрещено.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AizarkVariableEffectComponent : Component
{
    /// <summary>Ключ успешного триггера, при котором выполняется операция.</summary>
    [DataField, AutoNetworkedField]
    public string Key = "trigger";

    /// <summary>Имя существующей переменной; новые ключи эффект не создаёт.</summary>
    [DataField(required: true), AutoNetworkedField]
    public string Variable = string.Empty;

    /// <summary>Операция над значением.</summary>
    [DataField, AutoNetworkedField]
    public AizarkVariableOperation Operation = AizarkVariableOperation.Subtract;

    /// <summary>Операнд (по умолчанию 1).</summary>
    [DataField, AutoNetworkedField]
    public int Value = 1;

    /// <summary>
    /// false: изменить свою переменную. true: изменить переменную пользователя
    /// события. При отсутствии пользователя действие пропускается.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool TargetUser;
}
