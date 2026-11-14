using Content.Shared.VendingMachines.Components;

namespace Content.Shared.VendingMachines;

public abstract partial class SharedVendingMachineSystem
{
    private void AddInventoryExtensions(EntityUid uid, VendingMachineComponent component, float restockQuality)
    {
        foreach (var extension in _prototypeManager.EnumeratePrototypes<VendingMachineExtensionPrototype>())
        {
            if (extension.TargetPack != component.PackPrototypeId)
                continue;

            AddInventoryFromPrototype(uid, extension.StartingInventory, InventoryType.Regular, component, restockQuality);
            AddInventoryFromPrototype(uid, extension.EmaggedInventory, InventoryType.Emagged, component, restockQuality);
            AddInventoryFromPrototype(uid, extension.ContrabandInventory, InventoryType.Contraband, component, restockQuality);
        }
    }
}
