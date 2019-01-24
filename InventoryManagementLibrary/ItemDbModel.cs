namespace InventoryManagementLibrary
{
    public class ItemDbModel
    {
        public int ItemId { get; set; }
        public string GenericName { get; set; }
        public string BrandName { get; set; }
        public string Manufacturer { get; set; }
        public string DosageStrength { get; set; }
        public string DosageForm { get; set; }
        public int TherapeuticClassId { get; set; }
        public string Barcode { get; set; }
        public int Price { get; set; }
    }
}