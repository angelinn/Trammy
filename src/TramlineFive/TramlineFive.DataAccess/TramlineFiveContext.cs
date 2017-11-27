﻿using SQLite;
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
                await db.CreateTableAsync<Favourite>();
            }
        }

        public static async Task AddAsync(object entity)
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            await db.InsertAsync(entity);
        }

        public static async Task<IEnumerable<T>> Take<T>(int count = 10) where T : new()
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            return await db.Table<T>().Take(10).ToListAsync();
        }

        public static async Task CleanHistoryAsync()
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            await db.ExecuteAsync("DELETE FROM History");
        }
    }
}
