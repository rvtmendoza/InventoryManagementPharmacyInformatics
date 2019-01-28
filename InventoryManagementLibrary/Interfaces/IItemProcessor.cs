using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementLibrary
{
    public interface IItemProcessor
    {
        Task<IEnumerable<ItemDbModel>> GetItemsById(IEnumerable<int> itemIds);
        Task<IEnumerable<ItemDbModel>> GetItems(string genericName = "", string brandName = "",
            string manufacturer = "", string barcode = "",
            TherapeuticClassDbModel therapeuticClass = null);
        Task<int> AddItem(ItemDbModel item);
    }
}