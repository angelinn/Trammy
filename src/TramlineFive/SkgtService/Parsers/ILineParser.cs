using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Parsers
{
    public interface ILineParser
    {
        Task<IEnumerable<Line>> GetLinesAsync(TransportType type);
        Task<IEnumerable<Direction>> GetDirectionsAsync(Line line);

    }
}
    