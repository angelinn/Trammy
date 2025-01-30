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
        private static bool loaded;
        public static string DatabasePath { get; set; }
        public static async Task EnsureCreatedAsync()
        {
            if (loaded)
                return;

            if (!File.Exists(DatabasePath))
            {
                SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
                await db.CreateTableAsync<History>();
                await db.CreateTableAsync<Favourite>();
            }

            loaded = true;
        }

        public static async Task<Favourite> FindFavouriteAsync(string stopCode)
        {
            await EnsureCreatedAsync();

            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            return await db.FindAsync<Favourite>(f => f.StopCode == stopCode);
        }

        public static async Task RemoveFavouriteAsync(string stopCode)
        {
            await EnsureCreatedAsync();

            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);

            Favourite target = await FindFavouriteAsync(stopCode);
            if (target != null)
                await db.DeleteAsync(target);
        }

        public static async Task AddAsync(object entity)
        {
            await EnsureCreatedAsync();

            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            await db.InsertAsync(entity);
        }

        public static async Task AddOrUpdateHistoryAsync(History history)
        {
            await EnsureCreatedAsync();

            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);

            History exists = await db.FindAsync<History>(h => h.StopCode == history.StopCode);
            if (exists is null)
                await AddAsync(history);
            else
            {
                exists.TimeStamp = DateTime.Now;
                await db.UpdateAsync(exists);
            }
        }

        public static async Task IncrementFavouriteAsync(string stopCode)
        {
            await EnsureCreatedAsync();

            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            Favourite favourite = await db.FindAsync<Favourite>(f => f.StopCode == stopCode);

            ++favourite.TimesClicked;
            await db.UpdateAsync(favourite);
        }

        public static async Task<IEnumerable<T>> Take<T>(int count = 10) where T : new()
        {
            await EnsureCreatedAsync();

            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            return await db.Table<T>().Take(10).ToListAsync();
        }

        public static async Task<IEnumerable<T>> TakeAll<T>() where T : new()
        {
            await EnsureCreatedAsync();

            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            return await db.Table<T>().ToListAsync();
        }

        public static async Task<IEnumerable<T>> TakeByDescending<T, U>(System.Linq.Expressions.Expression<Func<T, U>> func, int count = 10) where T : new()
        {
            await EnsureCreatedAsync();

            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            return await db.Table<T>().OrderByDescending(func).Take(10).ToListAsync();
        }

        public static async Task<List<History>> TakeForLastDays(int days)
        {
            await EnsureCreatedAsync();

            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);

            DateTime limit = DateTime.Now.Subtract(TimeSpan.FromDays(days));
            return await db.Table<History>().Where(h => h.TimeStamp > limit).ToListAsync();
        }

        public static async Task CleanHistoryAsync()
        {
            await EnsureCreatedAsync();

            SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
            await db.ExecuteAsync("DELETE FROM History");
        }
    }
}
