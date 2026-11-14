using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Aizark.Api.Components;

/// <summary>
/// Универсальный эффект замены сущности по штатному событию TriggerEvent.
/// Используется для превращений предметов, смены оболочек и одноразовых устройств.
/// </summary>
/// <remarks>
/// Компонент не знает о конкретных игровых предметах и не изменяет TriggerSystem.
/// Срабатывание выполняется только сервером. Создаёт новый прототип в текущих
/// координатах владельца, затем удаляет исходную сущность. Состояние исходных
/// компонентов и содержимое контейнеров не переносятся автоматически.
/// Для замены с переносом содержимого требуется отдельный специализированный эффект.
/// </remarks>
[RegisterComponent]
public sealed partial class AizarkReplaceOnTriggerComponent : Component
{
    /// <summary>Ключ существующего триггера SS14.</summary>
    [DataField]
    public string Key = "trigger";

    /// <summary>Прототип, которым заменяется сущность после события.</summary>
    [DataField(required: true)]
    public EntProtoId Prototype = string.Empty;
}
