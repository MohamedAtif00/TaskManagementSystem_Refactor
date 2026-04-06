using System.Data;
using System.Data.Common;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.Tasks;

namespace AutomatedTaskSystem.Helper
{
    public static class DbConnectionExtensions
    {
        private static readonly DbConnection _connection;
        private static readonly DataContext _dataContext;

        /// <summary>
        /// Ensures the connection is open in an async-safe manner.
        /// </summary>
        public static async Task<DbConnection> EnsureOpenAsync(this DbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            return connection;
        }

        //public async static Task<IEnumerable<UserDto>> GetActiveUsersRawAsync()
        //{
        //    // 1. Get the connection from EF Context
        //    var connection = _dataContext.Database.GetDbConnection();

        //    // 2. Use the helper (clean, one-liner)
        //    await connection.EnsureOpenAsync();

        //    // 3. Execute your logic
        //    using var command = connection.CreateCommand();
        //    command.CommandText = "SELECT * FROM Users WHERE IsActive = 1";

        //    // ... rest of the execution logic
        //    return users;
        //}
    }
}
