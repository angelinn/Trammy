using System.Collections.Generic;
using TramlineFive.DataAccess;
using TramlineFive.DataAccess.Entities.GTFS;

namespace SkgtService;

public class GTFSRepository
{
    public List<StopWithType> Stops { get; private set; } = new();

    public void LoadStops()
    {
        Stops = GTFSContext.GetActiveStopsWithTypes();
    }
}
