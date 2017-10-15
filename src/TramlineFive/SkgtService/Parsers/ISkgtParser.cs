using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Parsers
{
    internal interface ISkgtParser
    {
        Task<List<Line>> GetLinesForStopAsync(string stopCode);
        void ChooseLine(Line target);
        string[] GetTimings(string captcha);
    }
}
