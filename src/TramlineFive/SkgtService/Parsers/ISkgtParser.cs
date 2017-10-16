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
        Task<string> ChooseLineAsync(Line target);
        string[] GetTimings(string captcha);
    }
}
