using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Parsers
{
    public interface ILineParser
    {
        Task<IEnumerable<SkgtObject>> GetLinesAsync(TransportType type);
        Task<IEnumerable<SkgtObject>> GetDirectionsAsync(SkgtObject line);
        Task<IEnumerable<SkgtObject>> GetStopsAsync(SkgtObject direction);
    }
}
    