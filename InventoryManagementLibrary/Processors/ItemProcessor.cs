using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementLibrary
{
    public class ItemProcessor : IItemProcessor
    {
        private readonly string _connectionString;

        public ItemProcessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<ItemDbModel>> GetItemsById(IEnumerable<int> itemIds)
        {
            var searchParameter = string.Join(",", itemIds);

            var query = $"SELECT * FROM Item WHERE ItemId IN ({searchParameter})";

            return await SqliteDataAccess.ExecuteQueryAsync<ItemDbModel>(_connectionString, query);
        }

        public async Task<IEnumerable<ItemDbModel>> GetItems(string genericName = "", string brandName = "", string manufacturer = "", string barcode = "", 
            TherapeuticClassDbModel therapeuticClass = null)
        {
            var searchParameters = GetSearchParameters(genericName, brandName, manufacturer, barcode, therapeuticClass);

            string query = "";
            if (searchParameters.Count == 0)
            {
                query = "SELECT * FROM Item";
            }
            else
            {
                var searchQuery = string.Join(" AND ", searchParameters);

                query = $"SELECT * FROM Item WHERE {searchQuery}";
            }

            return await SqliteDataAccess.ExecuteQueryAsync<ItemDbModel>(_connectionString, query);
        }

        public async Task<int> AddItem(ItemDbModel item)
        {
            var query =
                $"INSERT INTO Item (GenericName, BrandName, DosageForm, DosageStrength, Manufacturer, Barcode, Price, TherapeuticClassId) " +
                $"VALUES ('{item.GenericName}','{item.BrandName}','{item.DosageForm}','{item.DosageStrength}','{item.Manufacturer}','{item.Barcode}',{item.Price},{item.TherapeuticClassId})";

            return await SqliteDataAccess.ExecuteNonQueryAsync(_connectionString, query);
        }

        private List<string> GetSearchParameters(string genericName, string brandName, string manufacturer,
            string barcode, TherapeuticClassDbModel therapeuticClass)
        {
            List<string> searchParameters = new List<string>();

            if (!string.IsNullOrWhiteSpace(genericName))
            {
                searchParameters.Add($"GenericName = '{genericName}'");
            }

            if (!string.IsNullOrWhiteSpace(brandName))
            {
                searchParameters.Add($"BrandName = '{brandName}'");
            }

            if (!string.IsNullOrWhiteSpace(manufacturer))
            {
                searchParameters.Add($"Manufacturer = '{manufacturer}'");
            }

            if (!string.IsNullOrWhiteSpace(barcode))
            {
                searchParameters.Add($"Barcode = '{barcode}'");
            }

            if (therapeuticClass != null)
            {
                searchParameters.Add($"TherapeuticClassId = {therapeuticClass.TherapeuticClassId}");
            }

            return searchParameters;
        }
    }
}