using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementLibrary
{
    public interface IInventoryProcessor
    {
        Task<IEnumerable<InventoryDbModel>> GetInventoryItemsById(IEnumerable<int> itemIds, string batchNumber = "", string lotNumber = "");
        Task<int> AddInventoryItem(InventoryDbModel inventoryItem);
        Task<int> UpdateInventoryItem(InventoryDbModel inventoryItem);
    }
}