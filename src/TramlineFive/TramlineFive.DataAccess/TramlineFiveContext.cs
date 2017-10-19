using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Entities;

namespace TramlineFive.DataAccess
{
    public class TramlineFiveContext : DbContext
    {
        public DbSet<History> History { get; set; }
        private string dbPath;

        public TramlineFiveContext(string databasePath)
        {
            dbPath = databasePath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }
    }
}
