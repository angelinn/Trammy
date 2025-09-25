using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using TramlineFive.DataAccess.Entities.GTFS;

namespace TramlineFive.DataAccess;

public class GTFSContext
{
    public static string DatabasePath { get; set; }

    public List<Stop> GetStops()
    {
        SQLiteConnection db = new SQLiteConnection(DatabasePath);
        return db.Table<Stop>().ToList();
    }
}
