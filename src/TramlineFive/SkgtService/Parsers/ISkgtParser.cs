using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Parsers
{
    public interface ISkgtParser
    {
        Task<List<Line>> GetLinesForStopAsync(string stopCode);
        Task<Captcha> ChooseLineAsync(Line target);
        Task<IEnumerable<string>> GetTimings(Line line, string captcha);
    }
}
