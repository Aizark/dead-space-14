using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.VendingMachines;

/// <summary>
/// Дополнительные товары для существующего ассортимента торгового автомата.
/// </summary>
[Prototype]
public sealed partial class VendingMachineExtensionPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("targetPack", required: true)]
    public ProtoId<VendingMachineInventoryPrototype> TargetPack;

    [DataField("startingInventory", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<uint, EntityPrototype>))]
    public Dictionary<string, uint> StartingInventory { get; private set; } = new();

    [DataField("emaggedInventory", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<uint, EntityPrototype>))]
    public Dictionary<string, uint>? EmaggedInventory { get; private set; }

    [DataField("contrabandInventory", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<uint, EntityPrototype>))]
    public Dictionary<string, uint>? ContrabandInventory { get; private set; }
}
