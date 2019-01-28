using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementLibrary
{
    public class InventoryProcessor : IInventoryProcessor
    {
        private readonly string _connectionString;

        public InventoryProcessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<InventoryDbModel>> GetInventoryItemsById(IEnumerable<int> itemIds, string batchNumber = "", string lotNumber = "")
        {
            var itemIdSearchParameter = string.Join(",", itemIds);
            var searchParameters = GetSearchParameter(itemIdSearchParameter, batchNumber, lotNumber);

            var searchParameterQuery = string.Join(" AND ", searchParameters);

            if (string.IsNullOrWhiteSpace(searchParameterQuery)) return null;

            var query =
                $"SELECT ItemId, BatchNumber, LotNumber, SUM(Quantity) AS Quantity, ManufacturingDate, ExpiryDate " +
                $"FROM Inventory " +
                $"WHERE {searchParameterQuery} " +
                $"GROUP BY ItemId, BatchNumber, LotNumber, ManufacturingDate, ExpiryDate";

            return await SqliteDataAccess.ExecuteQueryAsync<InventoryDbModel>(_connectionString, query);
        }

        public async Task<int> AddInventoryItem(InventoryDbModel inventoryItem)
        {
            var query =
                $"INSERT INTO Inventory (ItemId, BatchNumber, LotNumber, ExpiryDate, ManufacturingDate, Quantity) " +
                $"VALUES ({inventoryItem.ItemId},'{inventoryItem.BatchNumber}','{inventoryItem.LotNumber}','{inventoryItem.ExpiryDate.ToString("yyyy-MM-dd")}','{inventoryItem.ManufacturingDate.ToString("yyyy-MM-dd")}', {inventoryItem.Quantity})";

            return await SqliteDataAccess.ExecuteNonQueryAsync(_connectionString, query);
        }

        private List<string> GetSearchParameter(string itemIdSearchParameter, string batchNumber, string lotNumber)
        {
            List<string> searchParameters = new List<string>();

            if (!string.IsNullOrWhiteSpace(batchNumber))
            {
                searchParameters.Add($"BatchNumber = {batchNumber}");
            }

            if (!string.IsNullOrWhiteSpace(lotNumber))
            {
                searchParameters.Add($"LotNumber = {lotNumber}");
            }

            if (!string.IsNullOrWhiteSpace(itemIdSearchParameter))
            {
                searchParameters.Add($"ItemId IN ({itemIdSearchParameter})");
            }

            return searchParameters;
        }
    }
}