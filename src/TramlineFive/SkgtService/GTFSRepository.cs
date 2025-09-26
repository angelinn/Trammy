using System.Collections.Generic;
using System.Threading.Tasks;
using TramlineFive.DataAccess;
using TramlineFive.DataAccess.Entities.GTFS;

namespace SkgtService;

public class GTFSRepository
{
    public List<StopWithType> Stops { get; private set; } = new();

    public async Task LoadStopsAsync()
    {
        Stops = await GTFSContext.GetActiveStopsWithTypesAsync();
    }
}
