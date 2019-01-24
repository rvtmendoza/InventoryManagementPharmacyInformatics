using InventoryManagementLibrary;

namespace InventoryManagementWpf
{
    public class Item
    {
        public Item(ItemDbModel itemModel, InventoryDbModel inventoryModel)
        {
            ItemModel = itemModel;
            InventoryModel = inventoryModel;
        }

        public ItemDbModel ItemModel { get; set; }
        public InventoryDbModel InventoryModel { get; set; }

        public string DisplayName =>
            $"{ItemModel.GenericName} ({ItemModel.BrandName}) {ItemModel.DosageStrength} {ItemModel.DosageForm}";

        public string TherapeuticClass => ItemModel.TherapeuticClassId.ToString();

        public int Stock => InventoryModel.Quantity;

        public decimal Price => ((decimal)ItemModel.Price/100);
    }
}