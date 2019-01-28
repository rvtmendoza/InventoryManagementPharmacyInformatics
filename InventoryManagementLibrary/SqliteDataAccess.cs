using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;

namespace InventoryManagementLibrary
{
    public static class SqliteDataAccess
    {
        public static bool HasConnection(string connectionString)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    connection.Close();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                } 
            }
        }

        public static async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string connectionString, string query)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                return await connection.QueryAsync<T>(query);
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(string connectionString, string query)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                return await connection.ExecuteAsync(query);
            }
        }
    }
}