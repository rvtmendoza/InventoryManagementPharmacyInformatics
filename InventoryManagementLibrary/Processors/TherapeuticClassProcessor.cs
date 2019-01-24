using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementLibrary
{
    public class TherapeuticClassProcessor : ITherapeuticClassProcessor
    {
        public async Task<IEnumerable<TherapeuticClassDbModel>> GetTherapeuticClass(string connectionString)
        {
            var query = "SELECT * FROM TherapeuticClass";

            return await SqliteDataAccess.ExecuteQueryAsync<TherapeuticClassDbModel>(connectionString, query);
        }
    }
}