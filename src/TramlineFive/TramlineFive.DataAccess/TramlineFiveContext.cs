using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Entities;

namespace TramlineFive.DataAccess
{
    public class TramlineFiveContext : DbContext
    {
        public static string ConnectionString { get; set; }
        public DbSet<History> History { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={ConnectionString}");
        }
    }
}
