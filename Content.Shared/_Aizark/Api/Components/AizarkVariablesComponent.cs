using Robust.Shared.GameStates;

namespace Content.Shared._Aizark.Api.Components;

/// <summary>
/// Хранилище целочисленных переменных, принадлежащих конкретной сущности.
/// Каждый предмет со своим компонентом имеет собственные значения:
/// две одинаковые коробки не разделяют общий счётчик.
/// </summary>
/// <remarks>
/// Данные задаются через YAML и изменяются только серверной логикой AizarkApi.
/// Компонент сетевой, поскольку условия штатных предсказываемых триггеров
/// могут проверяться на клиенте. Не храните здесь секреты, полномочия или
/// скрытые от игрока игровые данные: они могут быть отправлены клиенту.
/// Состояние относится к сущности, а не к глобальному пространству имён.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AizarkVariablesComponent : Component
{
    /// <summary>
    /// Именованные целочисленные значения. Например, remaining: 10.
    /// Отсутствующий ключ НЕ равнозначен нулю: проверка такого ключа
    /// в AizarkVariableCondition завершится отказом.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, int> Values = new();
}
