using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementLibrary
{
    public interface ITherapeuticClassProcessor
    {
        Task<IEnumerable<TherapeuticClassDbModel>> GetTherapeuticClass(string connectionString);
    }
}