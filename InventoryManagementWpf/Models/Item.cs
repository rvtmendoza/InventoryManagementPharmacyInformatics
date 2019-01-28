using InventoryManagementLibrary;

namespace InventoryManagementWpf
{
    public class Item
    {
        public Item(ItemDbModel itemModel, InventoryDbModel inventoryModel, TherapeuticClassDbModel therapeuticClass)
        {
            ItemModel = itemModel;
            InventoryModel = inventoryModel;
            TherapeuticClass = therapeuticClass;
        }

        public ItemDbModel ItemModel { get; set; }
        public InventoryDbModel InventoryModel { get; set; }
        public TherapeuticClassDbModel TherapeuticClass { get; set; }

        public string DisplayName =>
            $"{ItemModel.GenericName} ({ItemModel.BrandName}) {ItemModel.DosageStrength} {ItemModel.DosageForm}";

        public string TherapeuticClassName => TherapeuticClass.TherapeuticClassName;

        public int Stock => InventoryModel.Quantity;

        public decimal Price => ((decimal)ItemModel.Price/100);
    }
}