using System;

namespace InventoryManagementLibrary
{
    public class InventoryDbModel
    {
        public int InventoryId { get; set; }
        public int ItemId { get; set; }
        public string LotNumber { get; set; }
        public string BatchNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime ManufacturingDate { get; set; }
        public int Quantity { get; set; }
    }
}