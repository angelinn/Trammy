using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.DataAccess.Entities;

namespace TramlineFive.DataAccess
{
    public class TramlineFiveContext
    {
        public static string DatabasePath { get; set; }
        public static async Task EnsureCreatedAsync()
        {
            if (!File.Exists(DatabasePath))
            {
                SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
                await db.CreateTableAsync<History>();
            }
        }

        public static async Task AddHistoryAsync(History history)
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            await db.InsertAsync(history);
        }

        public static async Task<IEnumerable<History>> Take(int count = 10)
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            return await db.Table<History>().Take(10).ToListAsync();
        }
    }
}
