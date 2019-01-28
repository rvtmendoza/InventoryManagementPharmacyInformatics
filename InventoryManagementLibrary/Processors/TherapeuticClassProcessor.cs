using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementLibrary
{
    public class TherapeuticClassProcessor : ITherapeuticClassProcessor
    {
        private readonly string _connectionString;

        public TherapeuticClassProcessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<TherapeuticClassDbModel>> GetTherapeuticClass()
        {
            var query = "SELECT * FROM TherapeuticClass";

            return await SqliteDataAccess.ExecuteQueryAsync<TherapeuticClassDbModel>(_connectionString, query);
        }
    }
}